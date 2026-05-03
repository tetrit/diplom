using System;
using System.Collections.Generic;
using UnityEngine;
using Surveillance.Recognize;
using Surveillance.Settings;

namespace Surveillance.Events
{
    public class EventProcessingModule : MonoBehaviour
    {
        [SerializeField] private RecognizeManager recognizeManager;

        private List<BaseRuleSO> activeRules = new List<BaseRuleSO>();
        public event Action<SystemEvent> OnSystemEventGenerated;
        
        // Словари для хранения таймеров кулдауна и состояния правил (счетчиков кадров и т.д.)
        private Dictionary<string, float> _lastTriggerTimes = new Dictionary<string, float>();
        private Dictionary<string, RuleContext> _ruleContexts = new Dictionary<string, RuleContext>();

        private void Start()
        {
            if (recognizeManager == null) recognizeManager = FindObjectOfType<RecognizeManager>();
            if (recognizeManager != null) recognizeManager.onCameraDetectionsCompleted += ProcessDetections;

            if (ConfigurationManager.Instance != null)
            {
                ConfigurationManager.Instance.OnConfigurationChanged += OnSettingsChanged;
                activeRules = ConfigurationManager.Instance.CurrentConfig.EventRules;
            }
        }

        private void OnSettingsChanged(SystemConfigurationSO config)
        {
            activeRules = config.EventRules;
        }

        private void ProcessDetections(DetectionResult result)
        {
            foreach (var rule in activeRules)
            {
                if (rule == null || !rule.IsActive) continue;

                // Уникальный ключ для пары "Камера + Правило"
                string stateKey = $"{result.CameraId}_{rule.RuleName}";

                // Инициализируем контекст (состояние), если его еще нет
                if (!_ruleContexts.ContainsKey(stateKey))
                    _ruleContexts[stateKey] = new RuleContext();

                // Делегируем проверку самому правилу
                if (rule.Evaluate(result, _ruleContexts[stateKey], out List<BoundingBox> triggeringBoxes, out string eventClassName))
                {
                    // Проверка кулдауна
                    if (IsCooldownPassed(stateKey, rule.CooldownSeconds))
                    {
                        GenerateAndDispatchEvent(rule, result.CameraId, eventClassName, triggeringBoxes);
                        _lastTriggerTimes[stateKey] = Time.time;
                    }
                }
            }
        }

        private bool IsCooldownPassed(string key, float cooldown)
        {
            if (!_lastTriggerTimes.ContainsKey(key)) return true;
            return (Time.time - _lastTriggerTimes[key]) >= cooldown;
        }

        private void GenerateAndDispatchEvent(BaseRuleSO rule, int cameraId, string targetClass, List<BoundingBox> triggeringBoxes)
        {
            SystemEvent newEvent = new SystemEvent
            {
                EventId = Guid.NewGuid().ToString(), 
                Timestamp = DateTime.Now,
                CameraId = cameraId, 
                RuleName = rule.RuleName,
                TargetClassName = targetClass, 
                DetectedCount = triggeringBoxes.Count,
                TriggeringBoxes = triggeringBoxes
            };
            OnSystemEventGenerated?.Invoke(newEvent);
        }

        private void OnDestroy()
        {
            if (recognizeManager != null) recognizeManager.onCameraDetectionsCompleted -= ProcessDetections;
            if (ConfigurationManager.Instance != null) ConfigurationManager.Instance.OnConfigurationChanged -= OnSettingsChanged;
        }
    }
}