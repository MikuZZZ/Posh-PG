using System;
using System.Management.Automation;
using TTRider.PowerShellAsync;
using Npgsql;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PoshPG
{
    [Cmdlet(VerbsCommon.Get, "Database")]
    [OutputType(typeof(string))]
    public class GetDatabase : AsyncCmdlet
    {

        [Parameter]
        public string SessionId { get; set; }

        protected override async Task ProcessRecordAsync()
        {

            try
            {
                var currentSession = this.SessionState.PSVariable.GetValue("Global:PgSessions") as List<PgSession>;

                var conn = currentSession.Find(conn => conn.SessionId == Int32.Parse(SessionId)).Connection;

                await using (var cmd = new NpgsqlCommand("SELECT datname FROM pg_database WHERE datistemplate = false", conn))
                await using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                        WriteObject(reader.GetString(0));
            }
            catch (Exception e)
            {
                WriteObject(e);
            }

        }
    }
}
