using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Runtime.CompilerServices;
using Npgsql;
using TTRider.PowerShellAsync;

[assembly: InternalsVisibleTo("PoshPG.Tests")]

namespace PoshPG
{
    public class PGCmdlet : AsyncCmdlet, IDynamicParameters
    {
        internal RuntimeDefinedParameterDictionary DynamicParameters;

        [Parameter] public SwitchParameter Quiet { get; set; }

        internal string Session => DynamicParameters["Session"].Value as string;

        internal PgSession DefaultSession
        {
            get => SessionState.PSVariable.GetValue("Global:PgSessions:default") as PgSession;
            set => SessionState.PSVariable.Set(new PSVariable("Global:PgSessions:default", value,
                ScopedItemOptions.AllScope));
        }

        internal Dictionary<string, PgSession> SavedSessions
        {
            get => SessionState.PSVariable.GetValue("Global:PgSessions") as Dictionary<string, PgSession>;
            set => SessionState.PSVariable.Set(new PSVariable("Global:PgSessions", value, ScopedItemOptions.AllScope));
        }

        internal Dictionary<string, PgQuery> SavedQueries
        {
            get => SessionState.PSVariable.GetValue("Global:PgQuery") as Dictionary<string, PgQuery>;
            set => SessionState.PSVariable.Set(new PSVariable("Global:PgQuery", value, ScopedItemOptions.AllScope));
        }

        internal PgSession CurrentSession
        {
            get
            {
                PgSession session = null;

                if (Session != null)
                    session = SavedSessions[Session];
                else if (DefaultSession != null)
                    session = DefaultSession;
                else
                    throw new Exception("Either SessionName, SessionId or DefaultSession Required");

                if (session == null) throw new Exception("Session not exists");

                return session;
            }
        }

        internal NpgsqlConnection CurrentConnection
        {
            get
            {
                if (CurrentSession.Connection.State == ConnectionState.Closed)
                    CurrentSession.Connect();

                return CurrentSession.Connection;
            }
        }

        public object GetDynamicParameters()
        {
            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();

            var sessionParamAttr = new Collection<Attribute> { };
            sessionParamAttr.Add(new ParameterAttribute { Mandatory = false, HelpMessage = "Name of a PgSession" });

            if (SavedSessions != null)
                sessionParamAttr.Add(new ValidateSetAttribute(SavedSessions.Select(session => session.Key).ToArray()));

            runtimeDefinedParameterDictionary.Add(
                "Session",
                new RuntimeDefinedParameter("Session", typeof(string), sessionParamAttr)
            );

            DynamicParameters = runtimeDefinedParameterDictionary;
            return runtimeDefinedParameterDictionary;
        }
    }
}