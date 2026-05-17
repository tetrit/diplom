using UnityEngine;

namespace Surveillance.Logs
{[CreateAssetMenu(fileName = "LoggingProfile", menuName = "Surveillance/Logs/Logging Profile")]
    public class LoggingProfileSO : ScriptableObject
    {
        [Header("Настройки сохранения")]
        public bool SaveToFile = true;
        public string FileName = "surveillance_log.txt";

        [Header("Хранение в памяти (для просмотра)")][Tooltip("Максимальное количество записей в памяти для UI")]
        public int MaxMemoryLogs = 1000;

        [Header("Фильтрация входных данных")]
        public bool LogSystemEvents = true;       
        public bool LogRawDetections = false;     
    }
}