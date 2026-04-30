using System.Collections.Generic;
using UnityEngine;
using Surveillance.Recognize;

namespace Surveillance.Events.Rules
{
    [CreateAssetMenu(fileName = "SimpleRule", menuName = "Surveillance/Rules/Simple Rule")]
    public class SimpleRuleSO : BaseRuleSO
    {
        public string TargetClassName = "person";
        [Min(1)] public int MinimumObjectsCount = 1;
        

        public override bool Evaluate(DetectionResult result, RuleContext context,
            out List<BoundingBox> triggeringBoxes, out string eventClassName)
        {
            eventClassName = TargetClassName;
            triggeringBoxes = GetMatchedBoxes(result.Boxes);
            return triggeringBoxes.Count >= MinimumObjectsCount;
        }

        protected List<BoundingBox> GetMatchedBoxes(List<BoundingBox> allBoxes)
        {
            List<BoundingBox> matched = new List<BoundingBox>();
            foreach (var box in allBoxes)
            {
                if (box.ClassName == TargetClassName)
                    matched.Add(box);
            }

            
            return matched;
        }
    }
}