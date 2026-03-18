using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class YoloMonitorPanel : MonoBehaviour
{
    [SerializeField] private RawImage videoImage;
    [SerializeField] private YoloOverlayCanvas overlayCanvas;
    [SerializeField] private YoloRunner sourceRunner;

    private void Reset()
    {
        if (videoImage == null)
            videoImage = GetComponentInChildren<RawImage>(true);

        if (overlayCanvas == null)
            overlayCanvas = GetComponentInChildren<YoloOverlayCanvas>(true);
    }

    private void OnEnable()
    {
        Subscribe(sourceRunner);
        ApplyVideoTexture();
        RedrawOverlay();
    }

    private void OnDisable()
    {
        Unsubscribe(sourceRunner);
        ClearView();
    }

    private void LateUpdate()
    {
        ApplyVideoTexture();
    }

    public void SetSourceRunner(YoloRunner runner)
    {
        if (sourceRunner == runner)
        {
            ApplyVideoTexture();
            RedrawOverlay();
            return;
        }

        Unsubscribe(sourceRunner);
        sourceRunner = runner;
        Subscribe(sourceRunner);

        ApplyVideoTexture();
        RedrawOverlay();
    }

    private void Subscribe(YoloRunner runner)
    {
        if (runner == null)
            return;

        runner.DetectionsUpdated -= OnDetectionsUpdated;
        runner.DetectionsUpdated += OnDetectionsUpdated;
    }

    private void Unsubscribe(YoloRunner runner)
    {
        if (runner == null)
            return;

        runner.DetectionsUpdated -= OnDetectionsUpdated;
    }

    private void OnDetectionsUpdated(YoloRunner runner)
    {
        if (runner != sourceRunner)
            return;

        ApplyVideoTexture();
        RedrawOverlay();
    }

    private void ApplyVideoTexture()
    {
        if (videoImage == null)
            return;

        Texture desiredTexture = null;

        if (sourceRunner != null && sourceRunner.Source != null)
            desiredTexture = sourceRunner.Source.OutputTexture;

        if (videoImage.texture != desiredTexture)
            videoImage.texture = desiredTexture;
    }

    private void RedrawOverlay()
    {
        if (overlayCanvas == null)
            return;

        overlayCanvas.ClearBoxes();

        if (sourceRunner == null)
            return;

        var detections = sourceRunner.CurrentDetections;

        for (int i = 0; i < detections.Count; i++)
        {
            var d = detections[i];

            overlayCanvas.DrawBox(
                i,
                d.x1,
                d.y1,
                d.x2,
                d.y2,
                sourceRunner.InputWidth,
                sourceRunner.InputHeight,
                d.className,
                d.confidence
            );
        }
    }

    private void ClearView()
    {
        if (videoImage != null)
            videoImage.texture = null;

        overlayCanvas?.ClearBoxes();
    }
}