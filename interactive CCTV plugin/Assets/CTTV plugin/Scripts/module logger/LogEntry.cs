using System;

namespace Surveillance.Logs
{
    // Типы записей для фильтрации при анализе
    public enum LogCategory
    {
        System,         // Системные события (запуск, ошибки)
        EventTrigger,   // Срабатывание правил (тревога)
        RawDetection    // Сырые детекции (опционально, для глубокой отладки)
    }

    // Унифицированная структура записи журнала (согласно требованиям ВКР)
    [Serializable]
    public struct LogEntry
    {
        public DateTime Timestamp;       // Время регистрации
        public LogCategory Category;     // Тип зафиксированного действия
        public int SourceId;             // Источник сообщения (Camera ID)
        public string Message;           // Основной текст
        public string Details;           // Доп. данные (например, JSON с найденными объектами)

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Category}] [Cam: {SourceId}] {Message} | {Details}";
        }
    }
}