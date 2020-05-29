using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Management.Automation;
using Npgsql;
using TTRider.PowerShellAsync;

namespace PoshPG
{
    public class PGCmdlet : AsyncCmdlet, IDynamicParameters
    {
        protected RuntimeDefinedParameterDictionary DynamicParameters;

        [Parameter] public SwitchParameter Quiet = false;

        [Parameter(
            Mandatory = false,
            HelpMessage = "Session Id"
        )]
        [Alias("sid")]
        public string SessionId = null;

        public string SessionName => DynamicParameters["SessionName"].Value as string;

        public PgSession DefaultSession
        {
            get => SessionState.PSVariable.GetValue("Global:PgSessions:default") as PgSession;
            set => SessionState.PSVariable.Set(new PSVariable("Global:PgSessions:default", value,
                ScopedItemOptions.AllScope));
        }

        public List<PgSession> SavedSessions
        {
            get => SessionState.PSVariable.GetValue("Global:PgSessions") as List<PgSession>;
            set => SessionState.PSVariable.Set(new PSVariable("Global:PgSessions", value, ScopedItemOptions.AllScope));
        }

        public Dictionary<string, PgQuery> SavedQueries
        {
            get => SessionState.PSVariable.GetValue("Global:PgQuery") as Dictionary<string, PgQuery>;
            set => SessionState.PSVariable.Set(new PSVariable("Global:PgQuery", value, ScopedItemOptions.AllScope));
        }

        public PgSession CurrentSession
        {
            get
            {
                PgSession session = null;
                ;
                if (SessionId != null)
                    session = SavedSessions.Find(conn => conn.SessionId == int.Parse(SessionId));
                else if (SessionName != null)
                    session = SavedSessions.Find(conn => conn.SessionName == SessionName);
                else if (DefaultSession != null)
                    session = DefaultSession;
                else
                    throw new Exception("Either SessionName, SessionId or DefaultSession Required");

                if (session == null) throw new Exception("Session not exists");

                return session;
            }
        }

        public NpgsqlConnection CurrentConnection
        {
            get
            {
                if (CurrentSession.Connection.State == ConnectionState.Closed) CurrentSession.Connect();

                return CurrentSession.Connection;
            }
        }

        public object GetDynamicParameters()
        {
            var name = SavedSessions.Select(session => session.SessionName).ToArray();

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();

            var sessionNameAttr = new Collection<Attribute>
            {
                new ParameterAttribute {Mandatory = false, HelpMessage = "Session Alias"},
                new ValidateSetAttribute(name)
            };
            var sessionNameParam = new RuntimeDefinedParameter("SessionName", typeof(string), sessionNameAttr);
            runtimeDefinedParameterDictionary.Add("SessionName", sessionNameParam);

            DynamicParameters = runtimeDefinedParameterDictionary;
            return runtimeDefinedParameterDictionary;
        }
    }
}