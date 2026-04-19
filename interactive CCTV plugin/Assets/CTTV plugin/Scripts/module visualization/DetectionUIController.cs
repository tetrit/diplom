using Surveillance.Recognize;
using UnityEngine;

[RequireComponent(typeof(YoloOverlayCanvas))]
public class DetectionUIController : MonoBehaviour
{
    [SerializeField]private MonitorSource _monitorSource;
    private YoloOverlayCanvas _overlayCanvas;
    private RecognizeManager _recognizeManager;
    

    void Start()
    {
        _overlayCanvas = GetComponentInParent<YoloOverlayCanvas>();
        Debug.Assert(_overlayCanvas != null);
        
        // Находим менеджер распознавания
        _recognizeManager = FindObjectOfType<RecognizeManager>();

        if (_recognizeManager != null)
        {
            // Подписываемся на событие
            _recognizeManager.OnCameraDetectionsCompleted += OnDetectionsReceived;
        }
        else
        {
            Debug.LogError("DetectionUIController: RecognizeManager не найден на сцене!");
        }
    }

    // Этот метод вызывается каждый раз, когда нейросеть заканчивает кадр
    private void OnDetectionsReceived(DetectionResult result)
    {
        // Если результат пришел от чужой камеры — игнорируем
        if (result.CameraId != _monitorSource.TargetCameraId)
            return;

        // Прячем старые боксы из прошлого кадра
        _overlayCanvas.ClearBoxes();

        int boxIndex = 0;

        // Отрисовываем новые
        foreach (var box in result.Boxes)
        {
            // Проверяем, не вышли ли за лимит пула (Max Boxes)
            if (boxIndex >= _overlayCanvas.MaxBoxes)
                break;

            _overlayCanvas.DrawBox(
                boxIndex,
                box.X1, box.Y1, box.X2, box.Y2,
                result.FrameWidth, result.FrameHeight,
                box.ClassName,
                box.Confidence
            );

            boxIndex++;
        }
    }

    // Важно: отписываемся при уничтожении объекта, чтобы не было утечек!
    void OnDestroy()
    {
        if (_recognizeManager != null)
        {
            _recognizeManager.OnCameraDetectionsCompleted -= OnDetectionsReceived;
        }
    }

    // (Опционально) Метод для переключения камеры в рантайме
    public void SetTargetCamera(int newCameraId)
    {
        _monitorSource.TargetCameraId = newCameraId;
        _overlayCanvas.ClearBoxes(); // очищаем старые боксы при переключении
    }
}