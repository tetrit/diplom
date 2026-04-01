using UnityEngine;

namespace Surveillance.Recognition
{
    public sealed class RecognitionDebugListener : MonoBehaviour
    {
        private IRecognitionService recognitionService;

        private void Start()
        {
            if (!ServiceLocator.TryGet<IRecognitionService>(out recognitionService))
            {
                Debug.LogWarning("Recognition service not found.");
                return;
            }

            recognitionService.DetectionFrameProduced += OnDetectionFrameProduced;
        }

        private void OnDestroy()
        {
            if (recognitionService != null)
                recognitionService.DetectionFrameProduced -= OnDetectionFrameProduced;
        }

        private void OnDetectionFrameProduced(DetectionFrame frame)
        {
            Debug.Log(
                "[Recognition] camera=" + frame.CameraId +
                " detections=" + frame.Detections.Count +
                " time=" + frame.Timestamp.ToString("F3"));
        }
    }
}