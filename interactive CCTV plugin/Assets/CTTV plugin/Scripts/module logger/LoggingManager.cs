using System;
using System.Collections.Generic;
using Surveillance.Events;
using Surveillance.Recognize;
using UnityEngine;

namespace Surveillance.Logs
{
    public class LoggingManager : MonoBehaviour
    {[Header("Настройки")]
        [SerializeField] private LoggingProfileSO profile;

        [Header("Связи")]
        [SerializeField] private EventProcessingModule eventModule;
        [SerializeField] private RecognizeManager recognizeManager;
        
        private List<ILogStorage> _storages = new List<ILogStorage>();
        private MemoryLogStorage _memoryStorage;
        
        public event Action<LogEntry> OnNewLogAdded;

        private void Awake()
        {
            if (profile == null)
            {
                Debug.LogError("LoggingManager: Профиль настроек логгера не назначен!");
                return;
            }


            _memoryStorage = new MemoryLogStorage(profile.MaxMemoryLogs);
            _storages.Add(_memoryStorage);

            if (profile.SaveToFile)
            {
                _storages.Add(new FileLogStorage(profile.FileName));
                LogSystemMessage($"Запись в файл включена: {profile.FileName}");
            }
        }

        private void Start()
        {

            if (eventModule == null) eventModule = FindObjectOfType<EventProcessingModule>();
            if (recognizeManager == null) recognizeManager = FindObjectOfType<RecognizeManager>();


            if (eventModule != null)
                eventModule.OnSystemEventGenerated += HandleSystemEvent;

            if (recognizeManager != null)
                recognizeManager.onCameraDetectionsCompleted += HandleRawDetection;

            LogSystemMessage("Модуль журналирования успешно инициализирован.");
        }
        
        private void HandleSystemEvent(SystemEvent sysEvent)
        {
            if (!profile.LogSystemEvents) return;

  
            LogEntry entry = new LogEntry
            {
                Timestamp = sysEvent.Timestamp,
                Category = LogCategory.EventTrigger,
                SourceId = sysEvent.CameraId,
                Message = $"Сработало правило: '{sysEvent.RuleName}'",
                Details = $"Обнаружен класс: {sysEvent.TargetClassName}, Кол-во: {sysEvent.DetectedCount}"
            };

            DispatchLog(entry);
        }

        private void HandleRawDetection(DetectionResult result)
        {
            if (!profile.LogRawDetections) return;

    
            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = LogCategory.RawDetection,
                SourceId = result.CameraId,
                Message = $"Кадр обработан",
                Details = $"Найдено объектов: {result.Boxes.Count}"
            };

            DispatchLog(entry);
        }
        
        public void LogSystemMessage(string message)
        {
            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = LogCategory.System,
                SourceId = -1, 
                Message = message,
                Details = ""
            };
            DispatchLog(entry);
        }
      
        private void DispatchLog(LogEntry entry)
        {

            if (entry.Category == LogCategory.EventTrigger)
                Debug.LogWarning(entry.ToString()); 
            else
                Debug.Log(entry.ToString());
            
            foreach (var storage in _storages)
            {
                storage.Save(entry);
            }
            
            OnNewLogAdded?.Invoke(entry);
        }
        
        public IEnumerable<LogEntry> GetRecentLogs()
        {
            return _memoryStorage?.GetAllRecords();
        }

        private void OnDestroy()
        {
            if (eventModule != null)
                eventModule.OnSystemEventGenerated -= HandleSystemEvent;

            if (recognizeManager != null)
                recognizeManager.onCameraDetectionsCompleted -= HandleRawDetection;
        }
    }
}