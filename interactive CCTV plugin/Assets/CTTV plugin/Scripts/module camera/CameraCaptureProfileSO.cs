using UnityEngine;

namespace Surveillance.Cameras
{
    [CreateAssetMenu(
        fileName = "CameraCaptureProfile",
        menuName = "Surveillance/Camera Capture Profile")]
    public class CameraCaptureProfileSO : ScriptableObject
    {
        [Header("Render target")]
        [Min(64)] public int width = 640;
        [Min(64)] public int height = 360;
        [Min(0)] public int depthBits = 24;
        public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;

        [Header("Capture")]
        [Min(1)] public int targetCaptureFps = 10;
        public bool startStreaming = true;

        [Header("Camera params")]
        [Range(10f, 120f)] public float fieldOfView = 60f;
        [Min(0.01f)] public float nearClipPlane = 0.1f;
        [Min(1f)] public float farClipPlane = 1000f;
        public CameraClearFlags clearFlags = CameraClearFlags.Skybox;
        public Color backgroundColor = Color.black;
        public bool allowHdr = false;
        public bool allowMsaa = false;
    }
}