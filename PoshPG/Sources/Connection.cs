using System;
using System.Management.Automation;
using TTRider.PowerShellAsync;
using Npgsql;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PoshPG.Tests")]
namespace PoshPG
{
    [Cmdlet(VerbsCommon.New, "PgConnection")]
    [OutputType(typeof(string))]
    public class NewPgConnection : AsyncCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public string Endpoint { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public string Username { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public string Password { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public string Database { get; set; }

        [Parameter]
        public string Alias { get; set; }

        [Parameter]
        public string ConnectionString { get; set; }


        private void AddToSessionCollection(NpgsqlConnection connection)
        {
            var obj = new PgSession();
            var sessions = new List<PgSession>();

            var index = 0;

            var currentSession = this.SessionState.PSVariable.GetValue("Global:PgSessions") as List<PgSession>;

            if (currentSession != null && currentSession.Count > 0)
            {
                sessions.AddRange(currentSession);
                index = currentSession[currentSession.Count - 1].SessionId + 1;
            }

            obj.SessionId = index;
            obj.Connection = connection;
            obj.Alias = Alias;
            sessions.Add(obj);

            this.SessionState.PSVariable.Set((new PSVariable("Global:PgSessions", sessions, ScopedItemOptions.AllScope)));
        }

        internal async Task<NpgsqlConnection> Connect()
        {
            var connString = $"Host={Endpoint};Username={Username};Password={Password};Database={Database}";

            var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            return conn;
        }

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var conn = await Connect();
                AddToSessionCollection(conn);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }

        }
    }

    [Cmdlet(VerbsCommon.Get, "PgConnection")]
    [OutputType(typeof(string))]
    public class GetPgConnection : PSCmdlet
    {
        [Parameter]
        public string SessionId { get; set; }
        [Parameter]
        public string Alias { get; set; }

        protected override void ProcessRecord()
        {

            try
            {
                var currentSession = this.SessionState.PSVariable.GetValue("Global:PgSessions") as List<PgSession>;

                if (currentSession == null || currentSession.Count == 0)
                {
                    return;
                }

                WriteObject(currentSession);

            }
            catch (Exception e)
            {
                WriteObject(e);
            }

        }
    }
}
