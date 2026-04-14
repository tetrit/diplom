using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonitorManager))]
public class MonitorManagerEditor: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MonitorManager monitorManager = (MonitorManager)target;
        if (GUILayout.Button("Add Monitor"))
        {
            monitorManager.AddMonitor();
        }

        if (GUILayout.Button("Remove Monitor"))
        {
            monitorManager.RemoveMonitor(monitorManager.MonitorIDToRemove);
        }
    }
}
