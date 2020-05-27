using Xunit;
using Xunit.Abstractions;
using System.Data;
using Facebook.Yoga;

namespace PoshPG.Tests
{
    public class Test
    {
        private readonly ITestOutputHelper output;

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async void TestConnection()
        {
            var connectCmdlet = new NewPgConnection()
            {
                Endpoint = "psm-dev-1.cpq6vjqvr7b0.us-east-1.rds.amazonaws.com",
                Username = "pinon",
                Password = "Xg2Q8eWsgGm2G93typeUbKvu",
                Database = "pinon_food"
            };

            var conn = await connectCmdlet.Connect();

            Assert.Equal(ConnectionState.Open, conn.State);

            var queryCmdlet = new InvokePgQuery();
            var query = "select * from _user.user_info limit 1";

            var table = await queryCmdlet.GetQueryResult(conn, query);

            output.WriteLine("\n");
            output.WriteLine(table.ToString());
        }
    }
}
