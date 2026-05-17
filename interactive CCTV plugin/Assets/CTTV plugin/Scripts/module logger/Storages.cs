using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Surveillance.Logs
{

    public class MemoryLogStorage : ILogStorage
    {
        private readonly List<LogEntry> _logs = new List<LogEntry>();
        private readonly int _maxRecords;

        public MemoryLogStorage(int maxRecords)
        {
            _maxRecords = maxRecords;
        }

        public void Save(LogEntry entry)
        {
            _logs.Add(entry);
            if (_logs.Count > _maxRecords)
            {
                _logs.RemoveAt(0); 
            }
        }

        public IEnumerable<LogEntry> GetAllRecords() => _logs;
    }


    public class FileLogStorage : ILogStorage
    {
        private readonly string _filePath;

        public FileLogStorage(string fileName)
        {

            _filePath = Path.Combine(Application.dataPath, fileName);
            
 
            File.AppendAllText(_filePath, $"\n\n--- НАЧАЛО СЕССИИ {System.DateTime.Now} ---\n");
        }

        public void Save(LogEntry entry)
        {
            try
            {
                File.AppendAllText(_filePath, entry.ToString() + "\n");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Ошибка записи лога в файл: {ex.Message}");
            }
        }

        public IEnumerable<LogEntry> GetAllRecords()
        {
            throw new System.NotImplementedException("Для просмотра в UI используйте MemoryLogStorage");
        }
    }
}