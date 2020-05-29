using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Npgsql;

namespace PoshPG
{
    public class PgQuery
    {
        public string Query;
        public PgTableFormater Formatter { set; get; } = new PgTableDefaultFormater();

        public PgQuery(string query)
        {
            this.Query = query;
        }

        public string[] GetQueryParameters()
        {
            Regex rx = new Regex(@"\@([a-zA-z0-9_]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection matches = rx.Matches(Query);

            return matches.Select(m => m.Value.Substring(1)).Distinct().ToArray();
        }

        public async Task<string> Invoke(NpgsqlConnection conn, Dictionary<string, string> queryParams = null)
        {
            await using var cmd = new NpgsqlCommand(Query, conn);
            if (queryParams != null)
                foreach (var param in queryParams)
                    cmd.Parameters.Add(new NpgsqlParameter<string>(param.Key, param.Value));

            await cmd.PrepareAsync();
            var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return await Formatter.Format(reader);
        }
    }
}