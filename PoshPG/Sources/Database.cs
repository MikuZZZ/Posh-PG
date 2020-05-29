using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PoshPG
{
    [Cmdlet(VerbsCommon.Get, "PgDatabase")]
    [Alias("gpgd")]
    [OutputType(typeof(string))]
    public class GetPgDatabase : PGCmdlet
    {
        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var query = "select datname from pg_catalog.pg_database";
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