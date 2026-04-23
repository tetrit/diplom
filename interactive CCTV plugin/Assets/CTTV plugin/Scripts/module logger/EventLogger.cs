using UnityEngine;
using Surveillance.Events;

namespace Surveillance.Logs
{
    public class EventLogger : MonoBehaviour
    {
        private EventProcessingModule _eventModule;

        private void Start()
        {
            _eventModule = FindObjectOfType<EventProcessingModule>();
            if (_eventModule != null)
            {
                // Журнал слушает только ГОТОВЫЕ СОБЫТИЯ, а не сырые кадры!
                _eventModule.OnSystemEventGenerated += LogEvent;
            }
        }

        private void LogEvent(SystemEvent sysEvent)
        {
            // Форматируем красиво для журнала
            string logMessage = $"[ЖУРНАЛ СОБЫТИЙ | {sysEvent.Timestamp:HH:mm:ss}] " +
                                $"Камера: {sysEvent.CameraId} | " +
                                $"Правило: '{sysEvent.RuleName}' | " +
                                $"Обнаружено: {sysEvent.TargetClassName} (Кол-во: {sysEvent.DetectedCount})";

            Debug.LogWarning(logMessage); // Выводим желтым, чтобы выделялось в консоли
            
            // Здесь в будущем можно добавить:
            // File.AppendAllText("events_log.txt", logMessage + "\n");
        }

        private void OnDestroy()
        {
            if (_eventModule != null)
                _eventModule.OnSystemEventGenerated -= LogEvent;
        }
    }
}