using System.Collections.Generic;

namespace Surveillance.Logs
{

    public interface ILogStorage
    {
        void Save(LogEntry entry);
        IEnumerable<LogEntry> GetAllRecords();
    }
}