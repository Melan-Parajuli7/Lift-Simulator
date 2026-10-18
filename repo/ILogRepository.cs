using System.Collections.Generic;

namespace ElevatorApp
{
    public interface ILogRepository
    {
        void Add(string action);
        List<LogEntry> GetRecent(int count);
        void Clear();
    }
}
