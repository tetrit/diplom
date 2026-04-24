using System.Collections.Generic;

namespace Surveillance.Logs
{
    // Интерфейс для различных способов хранения журнала
    public interface ILogStorage
    {
        void Save(LogEntry entry);
        IEnumerable<LogEntry> GetAllRecords();
    }
}