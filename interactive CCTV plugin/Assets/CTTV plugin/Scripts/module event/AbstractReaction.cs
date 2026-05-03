using UnityEngine;
using Surveillance.Events;

namespace Surveillance.Reactions
{

    public abstract class AbstractReaction : MonoBehaviour
    {[Tooltip("На какое правило реагируем")]
        public string targetRuleName;

        protected virtual void Awake()
        {
            var eventModule = FindObjectOfType<EventProcessingModule>();
            if (eventModule != null)
            {
                eventModule.OnSystemEventGenerated += CheckAndExecute;
            }
        }

        private void CheckAndExecute(SystemEvent sysEvent)
        {
            if (sysEvent.RuleName == targetRuleName)
            {

                ExecuteReaction(sysEvent); 
            }
        }


        protected abstract void ExecuteReaction(SystemEvent sysEvent);

        protected virtual void OnDestroy()
        {
            var eventModule = FindObjectOfType<EventProcessingModule>();
            if (eventModule != null)
                eventModule.OnSystemEventGenerated -= CheckAndExecute;
        }
    }
}