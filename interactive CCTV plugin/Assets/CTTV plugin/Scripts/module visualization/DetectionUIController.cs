using Surveillance.Recognize;
using Surveillance.Settings;
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

        if (ConfigurationManager.Instance != null)
        {
            ConfigurationManager.Instance.OnConfigurationChanged += OnSettingsChanged;
            OnSettingsChanged(ConfigurationManager.Instance.CurrentConfig);
        }
    }

    // ИСПРАВЛЕНО: SystemConfigurationSO
    private void OnSettingsChanged(SystemConfigurationSO config)
    {
        if (_overlayCanvas != null)
        {
            _overlayCanvas.MaxBoxes = config.DisplaySettings.MaxBoxesOnScreen;
            _overlayCanvas.DefaultBoxColor = config.DisplaySettings.BoundingBoxColor;
        }
    }

    private void OnDetectionsReceived(DetectionResult result)
    {
        if (result.CameraId != _monitorSource.TargetCameraId) return;
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
        if (_recognizeManager != null) _recognizeManager.onCameraDetectionsCompleted -= OnDetectionsReceived;
        if (ConfigurationManager.Instance != null) ConfigurationManager.Instance.OnConfigurationChanged -= OnSettingsChanged;
    }
}