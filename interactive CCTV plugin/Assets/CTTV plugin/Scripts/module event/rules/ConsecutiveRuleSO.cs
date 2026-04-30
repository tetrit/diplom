using System.Collections.Generic;
using Surveillance.Events;
using Surveillance.Events.Rules;
using Surveillance.Recognize;
using UnityEngine;

namespace Surveillance.Events.Rules
{[CreateAssetMenu(fileName = "ConsecutiveRule", menuName = "Surveillance/Rules/Consecutive Rule")]
    public class ConsecutiveRuleSO : SimpleRuleSO
    {
        [Header("Удержание")]
        [Tooltip("Сколько кадров подряд объекты должны находиться в кадре")]
        [Min(1)] public int RequiredConsecutiveDetections = 3;

        public override bool Evaluate(DetectionResult result, RuleContext context, out List<BoundingBox> triggeringBoxes, out string eventClassName)
        {
            eventClassName = TargetClassName;
            triggeringBoxes = GetMatchedBoxes(result.Boxes);

            // Если в текущем кадре нужное кол-во объектов есть
            if (triggeringBoxes.Count >= MinimumObjectsCount)
            {
                context.ConsecutiveDetections++;
                if (context.ConsecutiveDetections >= RequiredConsecutiveDetections)
                {
                    // Правило сработало. Сбрасываем счетчик, чтобы избежать спама
                    context.ConsecutiveDetections = 0; 
                    return true;
                }
            }
            else
            {
                // Объекты пропали или их мало - сбрасываем цепочку
                context.ConsecutiveDetections = 0;
            }

            return false;
        }
    }
}