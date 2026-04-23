using System;
using System.Collections.Generic;
using UnityEngine;
using Surveillance.Recognize;
using Surveillance.Settings;

namespace Surveillance.Events
{
    public class EventProcessingModule : MonoBehaviour
    {[SerializeField] private RecognizeManager recognizeManager;

        // Теперь правила подтягиваются из файла, а не из редактора
        private List<RuleConfig> activeRules = new List<RuleConfig>();
        public event Action<SystemEvent> OnSystemEventGenerated;
        private Dictionary<string, float> _lastTriggerTimes = new Dictionary<string, float>();

        private void Start()
        {
            if (recognizeManager == null) recognizeManager = FindObjectOfType<RecognizeManager>();
            if (recognizeManager != null) recognizeManager.OnCameraDetectionsCompleted += ProcessDetections;

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

                List<BoundingBox> matchedBoxes = CheckRuleConditions(rule, result.Boxes);

                if (matchedBoxes.Count >= rule.MinimumObjectsCount)
                {
                    string cooldownKey = $"{result.CameraId}_{rule.RuleName}";
                    if (IsCooldownPassed(cooldownKey, rule.CooldownSeconds))
                    {
                        GenerateAndDispatchEvent(rule, result.CameraId, matchedBoxes);
                        _lastTriggerTimes[cooldownKey] = Time.time;
                    }
                }
            }
        }

        private List<BoundingBox> CheckRuleConditions(RuleConfig rule, List<BoundingBox> boxes)
        {
            List<BoundingBox> matched = new List<BoundingBox>();
            foreach (var box in boxes)
            {
                if (box.ClassName == rule.TargetClassName && box.Confidence >= rule.MinimumConfidence)
                    matched.Add(box);
            }
            return matched;
        }

        private bool IsCooldownPassed(string key, float cooldown)
        {
            if (!_lastTriggerTimes.ContainsKey(key)) return true;
            return (Time.time - _lastTriggerTimes[key]) >= cooldown;
        }

        private void GenerateAndDispatchEvent(RuleConfig rule, int cameraId, List<BoundingBox> triggeringBoxes)
        {
            SystemEvent newEvent = new SystemEvent
            {
                EventId = Guid.NewGuid().ToString(), Timestamp = DateTime.Now,
                CameraId = cameraId, RuleName = rule.RuleName,
                TargetClassName = rule.TargetClassName, DetectedCount = triggeringBoxes.Count,
                TriggeringBoxes = triggeringBoxes
            };
            OnSystemEventGenerated?.Invoke(newEvent);
        }

        private void OnDestroy()
        {
            if (recognizeManager != null) recognizeManager.OnCameraDetectionsCompleted -= ProcessDetections;
            if (ConfigurationManager.Instance != null) ConfigurationManager.Instance.OnConfigurationChanged -= OnSettingsChanged;
        }
    }
}