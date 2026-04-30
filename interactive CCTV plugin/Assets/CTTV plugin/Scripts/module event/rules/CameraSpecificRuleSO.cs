using System.Collections.Generic;
using Surveillance.Events;
using Surveillance.Events.Rules;
using Surveillance.Recognize;
using UnityEngine;

namespace Surveillance.Events.Rules
{[CreateAssetMenu(fileName = "CameraSpecificRule", menuName = "Surveillance/Rules/Camera Specific Rule")]
    public class CameraSpecificRuleSO : SimpleRuleSO
    {
        [Header("Привязка к камере")] public int TargetCameraId = 0;

        public override bool Evaluate(DetectionResult result, RuleContext context,
            out List<BoundingBox> triggeringBoxes, out string eventClassName)
        {
            eventClassName = TargetClassName;
            triggeringBoxes = new List<BoundingBox>();

            if (result.CameraId != TargetCameraId)
                return false; 

            triggeringBoxes = GetMatchedBoxes(result.Boxes);
            return triggeringBoxes.Count >= MinimumObjectsCount;
        }
    }
}