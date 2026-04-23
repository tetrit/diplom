using UnityEngine;

namespace Surveillance.Events
{[CreateAssetMenu(fileName = "New Detection Rule", menuName = "Surveillance/Events/Detection Rule")]
    public class DetectionRuleSO : ScriptableObject
    {[Header("Описание правила")]
        public string RuleName = "Обнаружение объекта";
        public bool IsActive = true;

        [Header("Условия срабатывания")][Tooltip("Название класса (из JSON), который мы ищем (например: person, car)")]
        public string TargetClassName;[Tooltip("Минимальное количество таких объектов в кадре для срабатывания")]
        [Min(1)] public int MinimumObjectsCount = 1;[Tooltip("Минимальная уверенность сети, чтобы мы поверили (0.1 - 1.0)")]
        [Range(0.1f, 1f)] public float MinimumConfidence = 0.5f;

        [Header("Настройки генерации")][Tooltip("Кулдаун (в секундах) между одинаковыми событиями на одной камере, чтобы не спамить в журнал каждый кадр")][Min(0f)] public float CooldownSeconds = 5f;
    }
}