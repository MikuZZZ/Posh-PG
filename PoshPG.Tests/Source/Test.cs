using System;
using System.Data;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using Xunit.Abstractions;

namespace PoshPG.Tests
{
    public class Test
    {
        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        private readonly ITestOutputHelper output;

        private SessionState PSSessionState;

        [Fact]
        public void TestConnection()
        {
            // var connectCmdlet = new NewPgSession
            // {
            //     Endpoint = "psm-dev-1.cpq6vjqvr7b0.us-east-1.rds.amazonaws.com",
            //     Username = "pinon",
            //     Password = "Xg2Q8eWsgGm2G93typeUbKvu",
            //     Database = "pinon_food",
            //     Name = "pg_dev"
            // };

            // var results = connectCmdlet.Invoke().OfType<string>().ToList();

            var connectCmdlet = new TestParameterSet();

            var results = connectCmdlet.Invoke().OfType<string>().ToList();

            // var getCmdlet = new GetPgSession();
            // getCmdlet.SavedSessions = connectCmdlet.SavedSessions;

            // var results = connectCmdlet.Invoke().OfType<string>().ToList();

            output.WriteLine(String.Join("\n", results.ToArray()));
        }
    }
}