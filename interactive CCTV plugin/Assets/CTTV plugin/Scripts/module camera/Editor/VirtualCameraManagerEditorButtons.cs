using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VirtualCameraManager))]
public class VirtualCameraManagerEditorButtons: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        VirtualCameraManager virtualCameraManager = (VirtualCameraManager)target;
        

        if (GUILayout.Button("Add VirtualCamera"))
        {
            virtualCameraManager.SpawnCameraEditor(virtualCameraManager.CameraPrefab);
        }

        if (GUILayout.Button("Remove VirtualCamera"))
        {
            virtualCameraManager.DestroyCameraEditor(virtualCameraManager.CameraIDToRemove);
        }
    }
}
