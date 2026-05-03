using Surveillance.Recognize;
using UnityEngine;

public class YoloOutputDebugger : MonoBehaviour
{
    private RecognizeManager _recognizeManager;

    void Awake()
    {
        _recognizeManager = FindObjectOfType<RecognizeManager>();
        if (_recognizeManager != null)
        {
            _recognizeManager.onCameraDetectionsCompleted += debug;
        }
    }

    public void debug(DetectionResult detectionResult)
    {
        foreach (var box in detectionResult.Boxes )
        {
            Debug.Log("Detection class: " + box.ClassName+ " --- Detection conf: " + box.Confidence);
        }
    }
}
