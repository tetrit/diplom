using System.Collections.Generic;
using UnityEngine;
using Surveillance.Recognize;

namespace Surveillance.Events
{
    // Класс для хранения текущего состояния правила для конкретной камеры
    public class RuleContext
    {
        public int ConsecutiveDetections = 0; // Подряд идущие кадры с детекцией
        public float FirstDetectionTime = 0f; // Время первой детекции (если нужно для расчета времени удержания)
    }

    public abstract class BaseRuleSO : ScriptableObject
    {[Header("Базовые настройки правила")]
        public string RuleName = "Новое правило";
        public bool IsActive = true;
        [Min(0f)] public float CooldownSeconds = 5f;

        // Метод, который будет переопределен в конкретных правилах.
        // Возвращает true, если событие должно сгенерироваться.
        // out triggeringBoxes - объекты, вызвавшие событие
        // out eventClassName - имя класса для записи в лог
        public abstract bool Evaluate(
            DetectionResult result, 
            RuleContext context, 
            out List<BoundingBox> triggeringBoxes, 
            out string eventClassName);
    }
}