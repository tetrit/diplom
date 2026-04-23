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

        // Хранилища логов
        private List<ILogStorage> _storages = new List<ILogStorage>();
        private MemoryLogStorage _memoryStorage;

        // Событие для обновления UI-просмотрщика
        public event Action<LogEntry> OnNewLogAdded;

        private void Awake()
        {
            if (profile == null)
            {
                Debug.LogError("LoggingManager: Профиль настроек логгера не назначен!");
                return;
            }

            // Инициализация хранилищ
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
            // Поиск зависимостей, если не назначены
            if (eventModule == null) eventModule = FindObjectOfType<EventProcessingModule>();
            if (recognizeManager == null) recognizeManager = FindObjectOfType<RecognizeManager>();

            // Подписка на источники данных (Рисунок 2.8 - Получение данных)
            if (eventModule != null)
                eventModule.OnSystemEventGenerated += HandleSystemEvent;

            if (recognizeManager != null)
                recognizeManager.OnCameraDetectionsCompleted += HandleRawDetection;

            LogSystemMessage("Модуль журналирования успешно инициализирован.");
        }

        // --- Обработчики данных от подсистем ---

        private void HandleSystemEvent(SystemEvent sysEvent)
        {
            if (!profile.LogSystemEvents) return;

            // Формирование записи журнала (Рисунок 2.8)
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

            // Запись каждого кадра распознавания (полезно для исследовательских целей/отладки сетей)
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

        // Вспомогательный метод для системных логов самого прототипа
        public void LogSystemMessage(string message)
        {
            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Category = LogCategory.System,
                SourceId = -1, // Система в целом
                Message = message,
                Details = ""
            };
            DispatchLog(entry);
        }

        // Сохранение и рассылка записей
        private void DispatchLog(LogEntry entry)
        {
            // Вывод в консоль Unity для разработчика
            if (entry.Category == LogCategory.EventTrigger)
                Debug.LogWarning(entry.ToString()); // Желтым, чтобы бросалось в глаза
            else
                Debug.Log(entry.ToString());

            // Сохранение во все подключенные хранилища (Память, Файл и т.д.)
            foreach (var storage in _storages)
            {
                storage.Save(entry);
            }

            // Уведомление UI (Предоставление данных для просмотра)
            OnNewLogAdded?.Invoke(entry);
        }

        // Метод для предоставления истории в UI
        public IEnumerable<LogEntry> GetRecentLogs()
        {
            return _memoryStorage?.GetAllRecords();
        }

        private void OnDestroy()
        {
            if (eventModule != null)
                eventModule.OnSystemEventGenerated -= HandleSystemEvent;

            if (recognizeManager != null)
                recognizeManager.OnCameraDetectionsCompleted -= HandleRawDetection;
        }
    }
}