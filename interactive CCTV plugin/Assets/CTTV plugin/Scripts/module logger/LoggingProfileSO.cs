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
        public bool LogSystemEvents = true;       // Логировать срабатывания правил[Tooltip("Осторожно: логирование сырых детекций создает очень много записей!")]
        public bool LogRawDetections = false;     // Логировать каждый кадр распознавания
    }
}