using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonitorSource))]
public class MonitorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MonitorSource monitorSource = (MonitorSource)target;

        if (GUILayout.Button("Apply settings"))
        {
            monitorSource.ApplySettings();
        }
    }
}
