using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PoshPG
{
    [Cmdlet(VerbsDiagnostic.Test, "ParameterSet")]
    [OutputType(typeof(string))]
    public class TestParameterSet : Cmdlet, IDynamicParameters
    {

        public class Generator : PGCmdlet, IValidateSetValuesGenerator
        {
            public string[] GetValidValues()
            {
                try
                {
                    return SavedSessions.Select(session => session.Key).ToArray();

                }
                catch
                {
                    return new string[] { "a", "c" };

                }
            }
        }

        [Parameter(Mandatory = true, ParameterSetName = "Set1")]
        public string Text;


        [Parameter(Mandatory = true, ParameterSetName = "Set2")]
        public string File;

        [Parameter(Mandatory = true, ParameterSetName = "Set4")]
        [ValidateSet(typeof(Generator))]
        public string Q;

        public object GetDynamicParameters()
        {
            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();

            var queryParamAttr = new Collection<Attribute> { };
            queryParamAttr.Add(new ParameterAttribute { Mandatory = true, ParameterSetName = "Set3" });

            runtimeDefinedParameterDictionary.Add(
                "Query",
                new RuntimeDefinedParameter("Query", typeof(string), queryParamAttr)
            );

            return runtimeDefinedParameterDictionary;
        }

        protected override void ProcessRecord()
        {
            var obj = new Generator();
            var result = obj.GetValidValues();
            WriteObject(result);
        }
    }

    [Cmdlet(VerbsLifecycle.Invoke, "PgQuery")]
    [OutputType(typeof(string))]
    public class InvokePgQuery : PGCmdlet, IDynamicParameters
    {
        [Parameter]
        public Hashtable Parameters;

        [Parameter(Mandatory = false)]
        public string Text;


        [Parameter(Mandatory = false)]
        public string File;

        public string Query => DynamicParameters["Query"].Value as string;

        new public object GetDynamicParameters()
        {
            base.GetDynamicParameters();

            var queryParamAttr = new Collection<Attribute> { };
            queryParamAttr.Add(new ParameterAttribute { Mandatory = false });

            if (SavedQueries != null)
                queryParamAttr.Add(new ValidateSetAttribute(SavedQueries.Select(query => query.Key).ToArray()));

            DynamicParameters.Add(
                "Query",
                new RuntimeDefinedParameter("Query", typeof(string), queryParamAttr)
            );

            return DynamicParameters;
        }

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var query = new PgQuery(Text);
                if (Query != null)
                    query = SavedQueries[Query];
                else if (File != null)
                    query = new PgQuery(File, System.Text.Encoding.UTF8);

                var paramDict = query
                    .GetQueryParameters()
                    .Select(p => new { Key = p, Value = null as string })
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                if (Parameters != null)
                    foreach (DictionaryEntry inputParams in Parameters)
                        paramDict[inputParams.Key.ToString()] = inputParams.Value.ToString();

                // WriteObject(p);
                var table = await query.Invoke(CurrentConnection, paramDict);
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
            await Task.Run(() =>
            {
                AddToQueryCollection(Name, new PgQuery(Query));

                var res = SavedQueries.Select(s => new { Name = s.Key, s.Value.Query });
                if (!Quiet)
                    WriteObject(res);
            });
        }
    }

    [Cmdlet(VerbsCommon.Get, "PgQuery")]
    [OutputType(typeof(string))]
    public class GetPgQuery : PGCmdlet
    {
        [Parameter] public string Name { get; set; }

        protected override async Task ProcessRecordAsync()
        {
            await Task.Run(() =>
            {
                var res = SavedQueries
                    .Where(s => Name == null || Name == s.Key)
                    .Select(s => new { Name = s.Key, s.Value.Query, Parameters = s.Value.GetQueryParameters() });

                if (!Quiet)
                    WriteObject(res);
            });
        }
    }
}