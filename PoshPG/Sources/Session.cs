using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Npgsql;

[assembly: InternalsVisibleTo("PoshPG.Tests")]

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

        [Parameter(Mandatory = false)] public string Alias { get; set; }

        [Parameter] public string ConnectionString { get; set; }


        private PgSession AddToSessionCollection(NpgsqlConnection connection)
        {
            var session = new PgSession();
            var sessions = new List<PgSession>();

            var index = 0;

            if (SavedSessions != null && SavedSessions.Count > 0)
            {
                sessions.AddRange(SavedSessions);
                index = SavedSessions[SavedSessions.Count - 1].SessionId + 1;
            }

            session.SessionId = index;
            session.Connection = connection;
            session.SessionName = Alias;
            sessions.Add(session);

            SavedSessions = sessions;
            return session;
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
                var session = AddToSessionCollection(conn);
                if (!Quiet) WriteObject(session);
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
            try
            {
                if (SavedSessions != null && SavedSessions.Count != 0) WriteObject(SavedSessions);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }
        }
    }

    [Cmdlet(VerbsCommon.Set, "PgDefaultSession")]
    [OutputType(typeof(string))]
    public class SetPgDefaultSession : PGCmdlet
    {
        protected override async Task ProcessRecordAsync()
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
        }
    }

    [Cmdlet(VerbsCommon.Get, "PgDefaultSession")]
    [OutputType(typeof(string))]
    public class GetPgDefaultSession : PGCmdlet
    {
        protected override async Task ProcessRecordAsync()
        {
            try
            {
                WriteObject(DefaultSession);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }
        }
    }
}