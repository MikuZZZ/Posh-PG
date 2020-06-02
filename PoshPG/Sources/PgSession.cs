using Npgsql;

namespace PoshPG
{
    public class PgSession
    {
        public NpgsqlConnection Connection;

        public PgSession(NpgsqlConnection Connection)
        {
            this.Connection = Connection;
        }

        public string Host => Connection.Host;

        public string Username => Connection.UserName;

        public string Database => Connection.Database;

        public string Status => Connection.State.ToString();

        public void Connect()
        {
            Connection.Open();
        }

        public void Disconnect()
        {
            Connection.Close();
        }
    }
}