using System;
using Npgsql;

namespace PoshPG
{
    public class PgSession
    {
        public Int32 SessionId;
        public string SessionName;
        public NpgsqlConnection Connection;

        public string Host
        {
            get { return Connection.Host; }
        }
        public string Username
        {
            get { return Connection.UserName; }
        }
        public string Database
        {
            get { return Connection.Database; }
        }
        public string Status
        {
            get { return Connection.State.ToString(); }
        }

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