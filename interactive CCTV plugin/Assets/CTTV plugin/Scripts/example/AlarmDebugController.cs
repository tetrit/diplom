using Surveillance.Events;
using Surveillance.Reactions;
using UnityEngine;

public class AlarmDebugController : AbstractReaction
{


    protected override void ExecuteReaction(SystemEvent sysEvent)
    {
        Debug.Log("AlarmDebugController");
    }
    
}
