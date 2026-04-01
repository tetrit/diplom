using Unity.InferenceEngine;
using UnityEngine;


namespace Surveillance.Recognition
{
    [CreateAssetMenu(
        fileName = "YoloDetectorProfile",
        menuName = "Surveillance/Yolo Detector Profile")]
    public class YoloDetectorProfileSO : ScriptableObject
    {
        [Header("Model")]
        public ModelAsset modelAsset;
        public TextAsset labelsAsset;

        [Header("Backend")]
        public BackendType backendType = BackendType.GPUCompute;

        [Header("Input")]
        [Min(32)] public int inputWidth = 640;
        [Min(32)] public int inputHeight = 640;
        [Range(1, 4)] public int inputChannels = 3;

        [Header("Scheduling")]
        [Min(1)] public int targetInferenceFps = 5;
        public bool warmupOnStart = true;
        public bool scheduleOverMultipleFrames = true;
        [Min(1)] public int layersPerFrame = 32;

        [Header("Postprocess")]
        [Range(0f, 1f)] public float confidenceThreshold = 0.25f;
        [Range(0f, 1f)] public float nmsIouThreshold = 0.45f;
        public YoloOutputLayout outputLayout = YoloOutputLayout.Auto;
        public YoloConfidenceMode confidenceMode = YoloConfidenceMode.Auto;
    }
}