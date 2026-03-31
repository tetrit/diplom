using UnityEngine;

namespace Surveillance.Monitors
{
    [CreateAssetMenu(
        fileName = "VirtualMonitorProfile",
        menuName = "Surveillance/Virtual Monitor Profile")]
    public class VirtualMonitorProfileSO : ScriptableObject
    {
        [Header("Source")]
        public VirtualMonitorSourceMode sourceMode = VirtualMonitorSourceMode.CameraStream;

        [Tooltip("ID камеры из VirtualCameraSource.CameraId")]
        public string cameraId = "camera_01";

        [Header("Behaviour")]
        public bool showFallbackWhenSourceMissing = true;
        public bool autoRebind = true;
        public bool startEnabled = true;
    }
}