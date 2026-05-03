using Surveillance.Recognize;
using UnityEngine;

[RequireComponent(typeof(YoloOverlayCanvas))]
public class DetectionUIController : MonoBehaviour
{
    [SerializeField] private MonitorSource _monitorSource;
    private YoloOverlayCanvas _overlayCanvas;
    private RecognizeManager _recognizeManager;

    void Start()
    {
        _overlayCanvas = GetComponent<YoloOverlayCanvas>();
        _recognizeManager = FindObjectOfType<RecognizeManager>();

        if (_recognizeManager != null)
            _recognizeManager.onCameraDetectionsCompleted += OnDetectionsReceived;
    }

    private void OnDetectionsReceived(DetectionResult result)
    {
        if (_monitorSource == null || result.CameraId != _monitorSource.TargetCameraId) return;
        
        _overlayCanvas.MaxBoxes = _monitorSource.MaxBoxesOnScreen;
        _overlayCanvas.DefaultBoxColor = _monitorSource.BoundingBoxColor;

        _overlayCanvas.ClearBoxes();

        int boxIndex = 0;
        foreach (var box in result.Boxes)
        {
            if (boxIndex >= _overlayCanvas.MaxBoxes) break;

            _overlayCanvas.DrawBox(
                boxIndex, box.X1, box.Y1, box.X2, box.Y2,
                result.FrameWidth, result.FrameHeight,
                box.ClassName, box.Confidence
            );
            boxIndex++;
        }
    }

    void OnDestroy()
    {
        if (_recognizeManager != null) 
            _recognizeManager.onCameraDetectionsCompleted -= OnDetectionsReceived;
    }
}