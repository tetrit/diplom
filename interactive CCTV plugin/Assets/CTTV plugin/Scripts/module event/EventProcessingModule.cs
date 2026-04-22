using System;
using System.Collections.Generic;
using UnityEngine;
using Surveillance.Recognize;

namespace Surveillance.Events
{
    public class EventProcessingModule : MonoBehaviour
    {
        [Header("Связь с подсистемами")]
        [Tooltip("Модуль распознавания (источник сырых данных)")]
        [SerializeField] private RecognizeManager recognizeManager;

        [Header("Активные правила (Модуль настроек)")]
        [SerializeField] private List<DetectionRuleSO> activeRules = new List<DetectionRuleSO>();

        // СОБЫТИЕ: Вызывается, когда сформировано осмысленное событие
        public event Action<SystemEvent> OnSystemEventGenerated;

        // Словарь для кулдаунов (защита от спама). Ключ: "CameraID_RuleName", Значение: Время последнего срабатывания
        private Dictionary<string, float> _lastTriggerTimes = new Dictionary<string, float>();

        private void Start()
        {
            if (recognizeManager == null)
                recognizeManager = FindObjectOfType<RecognizeManager>();

            if (recognizeManager != null)
            {
                // Подписываемся на "сырые" результаты распознавания
                recognizeManager.OnCameraDetectionsCompleted += ProcessDetections;
            }
            else
            {
                Debug.LogError("EventProcessingModule: Не найден RecognizeManager!");
            }
        }

        // --- БЛОК 1: Получение результатов распознавания ---
        private void ProcessDetections(DetectionResult result)
        {
            // Прогоняем полученные данные через все активные правила
            foreach (var rule in activeRules)
            {
                if (rule == null || !rule.IsActive) continue;

                // --- БЛОК 2: Проверка условий срабатывания ---
                List<BoundingBox> matchedBoxes = CheckRuleConditions(rule, result.Boxes);

                if (matchedBoxes.Count >= rule.MinimumObjectsCount)
                {
                    // Проверяем кулдаун, чтобы не создавать событие 10 раз в секунду
                    string cooldownKey = $"{result.CameraId}_{rule.RuleName}";
                    if (IsCooldownPassed(cooldownKey, rule.CooldownSeconds))
                    {
                        // --- БЛОК 3: Формирование события ---
                        GenerateAndDispatchEvent(rule, result.CameraId, matchedBoxes);
                        
                        // Обновляем таймер
                        _lastTriggerTimes[cooldownKey] = Time.time;
                    }
                }
            }
        }

        private List<BoundingBox> CheckRuleConditions(DetectionRuleSO rule, List<BoundingBox> boxes)
        {
            List<BoundingBox> matched = new List<BoundingBox>();

            foreach (var box in boxes)
            {
                // Совпадает ли класс и достаточно ли уверенности сети?
                if (box.ClassName == rule.TargetClassName && box.Confidence >= rule.MinimumConfidence)
                {
                    matched.Add(box);
                }
            }
            return matched;
        }

        private bool IsCooldownPassed(string key, float cooldown)
        {
            if (!_lastTriggerTimes.ContainsKey(key))
                return true;

            return (Time.time - _lastTriggerTimes[key]) >= cooldown;
        }

        private void GenerateAndDispatchEvent(DetectionRuleSO rule, int cameraId, List<BoundingBox> triggeringBoxes)
        {
            SystemEvent newEvent = new SystemEvent
            {
                EventId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                CameraId = cameraId,
                RuleName = rule.RuleName,
                TargetClassName = rule.TargetClassName,
                DetectedCount = triggeringBoxes.Count,
                TriggeringBoxes = triggeringBoxes
            };

            // --- БЛОК 4: Передача и регистрация события ---
            // Уведомляем все остальные модули (Журнал, UI и т.д.)
            OnSystemEventGenerated?.Invoke(newEvent);
        }

        private void OnDestroy()
        {
            if (recognizeManager != null)
                recognizeManager.OnCameraDetectionsCompleted -= ProcessDetections;
        }
    }
}