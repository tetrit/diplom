using System.Collections.Generic;
using UnityEngine;
using Surveillance.Recognize;

namespace Surveillance.Events
{

    public class RuleContext
    {
        public int ConsecutiveDetections = 0; 
        public float FirstDetectionTime = 0f; 
    }

    public abstract class BaseRuleSO : ScriptableObject
    {[Header("Базовые настройки правила")]
        public string RuleName = "Новое правило";
        public bool IsActive = true;
        [Min(0f)] public float CooldownSeconds = 5f;
        
        public abstract bool Evaluate(
            DetectionResult result, 
            RuleContext context, 
            out List<BoundingBox> triggeringBoxes, 
            out string eventClassName);
    }
}