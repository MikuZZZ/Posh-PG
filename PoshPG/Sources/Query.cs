using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PoshPG
{
    [Cmdlet(VerbsLifecycle.Invoke, "PgQuery")]
    [OutputType(typeof(string))]
    public class InvokePgQuery : PGCmdlet, IDynamicParameters
    {
        [Parameter] public string Query;

        [Parameter] public Hashtable QueryParameters;

        public new object GetDynamicParameters()
        {
            var dp = base.GetDynamicParameters() as RuntimeDefinedParameterDictionary;

            dp.Add("Name", new RuntimeDefinedParameter(
                "Name",
                typeof(string),
                new Collection<Attribute>
                {
                    new ParameterAttribute {Mandatory = false, HelpMessage = "Saved Query Name"},
                    new ValidateSetAttribute(SavedQueries.Select(s => s.Key).ToArray())
                }
            ));

            DynamicParameters = dp;
            return dp;
        }

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                // WriteObject(SavedQueries[DynamicParameters["Name"].Value as string].GetQueryParameters());
                // WriteObject(DynamicParameters.Select(dp => new { Name = dp.Key, Value = dp.Value.Value }));
                var query = Query;
                if (DynamicParameters["Name"] != null)
                    query = SavedQueries[DynamicParameters["Name"].Value as string].Query;

                var p = SavedQueries[DynamicParameters["Name"].Value as string]
                    .GetQueryParameters()
                    .Select(p => new {Key = p, Value = null as string})
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                if (QueryParameters != null)
                    foreach (DictionaryEntry inputParams in QueryParameters)
                        p[inputParams.Key.ToString()] = inputParams.Value.ToString();

                // WriteObject(p);
                var table = await new PgQuery(query).Invoke(CurrentConnection, p);
                WriteObject(table);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }
        }
    }

    [Cmdlet(VerbsCommon.New, "PgQuery")]
    [OutputType(typeof(string))]
    public class NewPgQuery : PGCmdlet
    {
        [Parameter(Mandatory = true)] public string Query { get; set; }


        [Parameter(Mandatory = true)] public string Name { get; set; }

        private void AddToQueryCollection(string name, PgQuery query)
        {
            var savedQuery = SavedQueries;
            if (savedQuery == null) savedQuery = new Dictionary<string, PgQuery>();

            savedQuery.Add(name, query);
            SavedQueries = savedQuery;
        }

        protected override async Task ProcessRecordAsync()
        {
            AddToQueryCollection(Name, new PgQuery(Query));

            var res = SavedQueries.Select(s => new {Name = s.Key, s.Value.Query});
            if (!Quiet) WriteObject(res);
        }
    }

    [Cmdlet(VerbsCommon.Get, "PgQuery")]
    [OutputType(typeof(string))]
    public class GetPgQuery : PGCmdlet, IDynamicParameters
    {
        public string Name { get; set; }

        public new object GetDynamicParameters()
        {
            var runtimeDefinedParameterDictionary = base.GetDynamicParameters() as RuntimeDefinedParameterDictionary;

            runtimeDefinedParameterDictionary.Add("Name", new RuntimeDefinedParameter(
                "Name",
                typeof(string),
                new Collection<Attribute>
                {
                    new ParameterAttribute {Mandatory = false, HelpMessage = "Saved Query Name"},
                    new ValidateSetAttribute(SavedQueries.Select(s => s.Key).ToArray())
                }
            ));

            return runtimeDefinedParameterDictionary;
        }

        protected override async Task ProcessRecordAsync()
        {
            var res = SavedQueries
                .Where(s => DynamicParameters["Name"].Value == null ||
                            (string) DynamicParameters["Name"].Value == s.Key)
                .Select(s => new {Name = s.Key, s.Value.Query, Parameters = s.Value.GetQueryParameters()});
            if (!Quiet) WriteObject(res);
        }
    }
}