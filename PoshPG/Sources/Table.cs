using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PoshPG
{
    [Cmdlet(VerbsCommon.Get, "PgTable")]
    [OutputType(typeof(string))]
    public class GetPgTable : PGCmdlet
    {
        [Parameter] public string Schema = null;

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var query =
                    "select * from information_schema.tables where (@p is null or table_schema = @p) and table_type = 'BASE TABLE'";
                var parameters = new Dictionary<string, string>
                {
                    ["p"] = Schema
                };

                var table = await new PgQuery(query).Invoke(CurrentConnection, parameters);
                WriteObject(table);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "PgTableInfo")]
    [OutputType(typeof(string))]
    public class GetPgTableInfo : PGCmdlet
    {
        [Parameter] public string Schema = null;

        [Parameter(Mandatory = true)] public string Table = null;

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var query = @"
                    SELECT * FROM information_schema.columns
                    WHERE table_name = @Table AND (@Schema is null or table_schema = @Schema)
                    ORDER BY ordinal_position; 
                ";

                var table = await new PgQuery(query).Invoke(CurrentConnection, new Dictionary<string, string>
                {
                    ["Table"] = Table,
                    ["Schema"] = Schema
                });

                WriteObject(table);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }
        }
    }
}