using Surveillance.Cameras;
using Surveillance.Recognize;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;

public class RecognizeManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private RecognizeProfileSO recognizeProfile;
    
    private YoloClassMapProvider yoloClassMapProvider;
    private VirtualCameraManager virtualCameraManager;
    private YoloInferenceEngine inferenceEngine;

    public event Action<DetectionResult> OnCameraDetectionsCompleted;

    private Queue<VirtualCameraSource> _processQueue = new Queue<VirtualCameraSource>();
    private bool _isProcessing = false;
    
    private Dictionary<int, float> _cameraNextDetectionTimes = new Dictionary<int, float>();
    private List<VirtualCameraSource> _activeCameras = new List<VirtualCameraSource>();

    void Awake()
    {
        virtualCameraManager = FindObjectOfType<VirtualCameraManager>();
        yoloClassMapProvider = GetComponent<YoloClassMapProvider>();
        
        if (virtualCameraManager != null)
        {
            // Подписываемся на создание и на удаление!
            virtualCameraManager.cameraInitializedEvent += OnCameraInitialized;
            virtualCameraManager.cameraRemovedEvent += OnCameraRemoved;
        }
    }

    void Start()
    {
        if (modelAsset != null && recognizeProfile != null)
            inferenceEngine = new YoloInferenceEngine(modelAsset, recognizeProfile, yoloClassMapProvider);

        // Собираем камеры, которые уже есть на сцене (например, расставлены вручную в редакторе)
        var existingCameras = FindObjectsByType<VirtualCameraSource>(FindObjectsSortMode.None);
        foreach (var cam in existingCameras)
        {
            OnCameraInitialized(cam);
        }
    }

    private void OnCameraInitialized(VirtualCameraSource cam)
    {
        if (cam == null) return;

        if (!_activeCameras.Contains(cam))
        {
            _activeCameras.Add(cam);
            _cameraNextDetectionTimes[cam.CameraId] = Time.time;
        }
    }

    // НОВЫЙ МЕТОД: Очищаем следы удаленной камеры
    private void OnCameraRemoved(int cameraId)
    {
        // 1. Убираем из таймеров
        if (_cameraNextDetectionTimes.ContainsKey(cameraId))
        {
            _cameraNextDetectionTimes.Remove(cameraId);
        }

        // 2. Убираем из активного списка (также чистим любые null-объекты на всякий случай)
        _activeCameras.RemoveAll(c => c == null || c.CameraId == cameraId);
    }

    void Update()
    {
        if (inferenceEngine == null || recognizeProfile == null) return;

        // Идем с конца, так как это безопасно
        for (int i = _activeCameras.Count - 1; i >= 0; i--)
        {
            var cam = _activeCameras[i];
            
            // Защита от Unity Fake Null (если удалили камеру минуя VirtualCameraManager, например, просто через Delete на сцене)
            if (cam == null)
            {
                _activeCameras.RemoveAt(i);
                continue; 
            }

            // Проверяем время и наличие текстуры
            if (Time.time >= _cameraNextDetectionTimes[cam.CameraId] && cam.OutputTexture != null)
            {
                _cameraNextDetectionTimes[cam.CameraId] = Time.time + recognizeProfile.detectionInterval;
                
                if (!_processQueue.Contains(cam) && _processQueue.Count < 3)
                {
                    _processQueue.Enqueue(cam);
                }
            }
        }

        if (!_isProcessing && _processQueue.Count > 0)
        {
            _ = ProcessQueueAsync();
        }
    }

    private async Task ProcessQueueAsync()
    {
        _isProcessing = true;

        while (_processQueue.Count > 0)
        {
            var cam = _processQueue.Dequeue();
            
            // Двойная проверка перед отправкой в нейросеть
            // Если камеру удалили, пока она ждала очереди - пропускаем!
            if (cam == null || cam.OutputTexture == null) continue;

            try
            {
                List<BoundingBox> foundBoxes = await inferenceEngine.RunInferenceAsync(cam.OutputTexture);

                // Еще одна проверка. Выполнение могло занять время (Await). 
                // Не удалили ли камеру ПОКА шла детекция?
                if (cam == null) continue;

                DetectionResult result = new DetectionResult
                {
                    CameraId = cam.CameraId,
                    FrameWidth = recognizeProfile.inputWidth,
                    FrameHeight = recognizeProfile.inputHeight,
                    Boxes = foundBoxes
                };

                OnCameraDetectionsCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                // Если произошла ошибка (например текстуру уничтожили во время чтения), игра не упадет
                Debug.LogWarning($"Распознавание камеры прервано/отменено: {ex.Message}");
            }
        }

        _isProcessing = false;
    }

    void OnDestroy()
    {
        if (virtualCameraManager != null)
        {
            virtualCameraManager.cameraInitializedEvent -= OnCameraInitialized;
            virtualCameraManager.cameraRemovedEvent -= OnCameraRemoved;
        }
        
        inferenceEngine?.Dispose();
    }
}