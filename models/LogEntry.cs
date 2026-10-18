using System;

namespace ElevatorApp
{
    /// <summary>One row of the activity log (table LiftLogs).</summary>
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Action { get; set; } = string.Empty;
    }
}
