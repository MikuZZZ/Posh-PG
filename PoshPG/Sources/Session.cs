using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Npgsql;

namespace PoshPG
{
    [Cmdlet(VerbsCommon.New, "PgSession")]
    [OutputType(typeof(string))]
    public class NewPgSession : PGCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Endpoint { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Username { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Password { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Database { get; set; }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        private void AddToSessionCollection(NpgsqlConnection connection)
        {
            var savedSession = SavedSessions;
            if (savedSession == null)
                savedSession = new Dictionary<string, PgSession>();

            savedSession.Add(Name, new PgSession(connection));
            SavedSessions = savedSession;
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

                if (!Quiet)
                {
                    WriteObject($"{Name} Created");
                    WriteObject(SavedSessions[Name]);
                }
            }
            catch (Exception e)
            {
                WriteObject(e);
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "PgSession")]
    [OutputType(typeof(string))]
    public class GetPgSession : PGCmdlet
    {
        protected override async Task ProcessRecordAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (SavedSessions != null && SavedSessions.Count != 0)
                        WriteObject(SavedSessions);
                }
                catch (Exception e)
                {
                    WriteObject(e);
                }
            });
        }
    }

    [Cmdlet(VerbsCommon.Set, "PgDefaultSession")]
    [OutputType(typeof(string))]
    public class SetPgDefaultSession : PGCmdlet
    {
        protected override async Task ProcessRecordAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    DefaultSession = CurrentSession;
                    WriteObject(DefaultSession);
                }
                catch (Exception e)
                {
                    WriteObject(e);
                }
            });
        }
    }

    [Cmdlet(VerbsCommon.Get, "PgDefaultSession")]
    [OutputType(typeof(string))]
    public class GetPgDefaultSession : PGCmdlet
    {
        protected override async Task ProcessRecordAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    WriteObject(DefaultSession);
                }
                catch (Exception e)
                {
                    WriteObject(e);
                }
            });
        }
    }
}