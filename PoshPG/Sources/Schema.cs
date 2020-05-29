using System;
using System.Management.Automation;
using TTRider.PowerShellAsync;
using Npgsql;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PoshPG
{
    [Cmdlet(VerbsCommon.Get, "PgSchema")]
    [OutputType(typeof(string))]
    public class GetPgSchema : PGCmdlet
    {
        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var query = "select * from information_schema.schemata;";

                var table = await new PgQuery(query).Invoke(CurrentConnection);
                WriteObject(table);
            }
            catch (Exception e)
            {
                WriteObject(e);
            }

        }
    }
}