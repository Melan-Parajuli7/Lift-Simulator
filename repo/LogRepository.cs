using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ElevatorApp
{
    /// <summary>Reads and writes the activity log in SQL Server (table LiftLogs).</summary>
    public class LogRepository : ILogRepository
    {
        private readonly DbConnectionFactory _factory;

        public LogRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(string action)
        {
            const string sql = "INSERT INTO dbo.LiftLogs (LogDate, LogTime, ActionText) VALUES (@date, @time, @action)";

            DateTime now = DateTime.Now;
            if (action.Length > 200) action = action.Substring(0, 200);

            using (SqlConnection conn = _factory.CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@date", SqlDbType.Date).Value = now.Date;
                cmd.Parameters.Add("@time", SqlDbType.Time).Value = new TimeSpan(now.Hour, now.Minute, now.Second);
                cmd.Parameters.Add("@action", SqlDbType.NVarChar, 200).Value = action;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<LogEntry> GetRecent(int count)
        {
            const string sql =
                "SELECT Id, LogDate, LogTime, ActionText FROM " +
                "(SELECT TOP (@count) Id, LogDate, LogTime, ActionText FROM dbo.LiftLogs ORDER BY Id DESC) AS recent " +
                "ORDER BY Id";

            var list = new List<LogEntry>();

            using (SqlConnection conn = _factory.CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@count", count);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LogEntry
                        {
                            Id = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            Time = reader.GetTimeSpan(2),
                            Action = reader.GetString(3)
                        });
                    }
                }
            }

            return list;
        }

        public void Clear()
        {
            using (SqlConnection conn = _factory.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM dbo.LiftLogs", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
