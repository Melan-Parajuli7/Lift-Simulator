using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace ElevatorApp
{
    /// <summary>Reads the connection string from app.config and creates SQL connections.</summary>
    public class DbConnectionFactory
    {
        public DbConnectionFactory()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["ElevatorDb"];
            if (settings == null)
                throw new InvalidOperationException("Connection string 'ElevatorDb' was not found in app.config.");

            ConnectionString = settings.ConnectionString;
        }

        public string ConnectionString { get; }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
