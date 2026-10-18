using Microsoft.Data.SqlClient;

namespace ElevatorApp
{
    /// <summary>Creates the database and the LiftLogs table automatically if they do not exist yet.</summary>
    public static class DbInitializer
    {
        public static void EnsureCreated(DbConnectionFactory factory)
        {
            var builder = new SqlConnectionStringBuilder(factory.ConnectionString);
            string databaseName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "ElevatorDB" : builder.InitialCatalog;

            // 1) create the database (connect to 'master' first)
            builder.InitialCatalog = "master";
            using (var master = new SqlConnection(builder.ConnectionString))
            {
                master.Open();
                using (SqlCommand cmd = master.CreateCommand())
                {
                    cmd.CommandText =
                        "IF DB_ID(@name) IS NULL " +
                        "BEGIN " +
                        "  DECLARE @sql NVARCHAR(400) = N'CREATE DATABASE ' + QUOTENAME(@name); " +
                        "  EXEC (@sql); " +
                        "END";
                    cmd.Parameters.AddWithValue("@name", databaseName);
                    cmd.ExecuteNonQuery();
                }
            }

            // 2) create the table
            using (SqlConnection conn = factory.CreateConnection())
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "IF OBJECT_ID(N'dbo.LiftLogs', N'U') IS NULL " +
                        "CREATE TABLE dbo.LiftLogs (" +
                        "  Id         INT IDENTITY(1,1) PRIMARY KEY, " +
                        "  LogDate    DATE          NOT NULL, " +
                        "  LogTime    TIME(0)       NOT NULL, " +
                        "  ActionText NVARCHAR(200) NOT NULL)";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
