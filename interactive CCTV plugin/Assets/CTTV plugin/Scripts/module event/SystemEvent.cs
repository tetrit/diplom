using System;
using System.Collections.Generic;
using Surveillance.Recognize;

namespace Surveillance.Events
{
    // Структура, описывающая уже свершившееся событие
    public struct SystemEvent
    {
        public string EventId;           // Уникальный ID события
        public DateTime Timestamp;       // Момент возникновения (реальное время)
        public int CameraId;             // Источник (какая камера)
        public string RuleName;          // Какое правило сработало (например "Тревога: Человек")
        public string TargetClassName;   // Класс объекта (person, car и т.д.)
        public int DetectedCount;        // Сколько объектов этого класса вызвало событие
        
        // Ссылка на конкретные боксы, которые вызвали событие (для визуализации/записи)
        public List<BoundingBox> TriggeringBoxes; 
    }
}