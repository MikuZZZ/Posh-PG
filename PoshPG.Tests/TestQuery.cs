using System.Data;
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

        [Fact]
        public async void TestConnection()
        {
            var connectCmdlet = new NewPgSession
            {
                Endpoint = "psm-dev-1.cpq6vjqvr7b0.us-east-1.rds.amazonaws.com",
                Username = "pinon",
                Password = "Xg2Q8eWsgGm2G93typeUbKvu",
                Database = "pinon_food"
            };

            var conn = await connectCmdlet.Connect();

            Assert.Equal(ConnectionState.Open, conn.State);

            var query = "select * from _user.user_info limit 1";
            var table = await new PgQuery(query).Invoke(conn);
            output.WriteLine(table);

            output.WriteLine("\n");
            output.WriteLine(table);
        }
    }
}