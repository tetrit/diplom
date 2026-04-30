using Surveillance.Cameras;
using Surveillance.Recognize;
using Surveillance.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RecognizeManager : MonoBehaviour
{
    private IClassMapProvider classMapProvider;
    private IInferenceEngine inferenceEngine;
    private VirtualCameraManager virtualCameraManager;

    public event Action<DetectionResult> onCameraDetectionsCompleted;

    private Queue<VirtualCameraSource> _processQueue = new Queue<VirtualCameraSource>();
    private bool _isProcessing = false;
    private Dictionary<int, float> _cameraNextDetectionTimes = new Dictionary<int, float>();
    private List<VirtualCameraSource> _activeCameras = new List<VirtualCameraSource>();

    private RecognitionConfig _currentConfig;

    void Awake()
    {
        virtualCameraManager = FindObjectOfType<VirtualCameraManager>();
        classMapProvider = GetComponent<IClassMapProvider>();
        
        if (virtualCameraManager != null)
        {
            virtualCameraManager.cameraInitializedEvent += OnCameraInitialized;
            virtualCameraManager.cameraRemovedEvent += OnCameraRemoved;
        }
    }

    void Start()
    {
        if (ConfigurationManager.Instance != null)
        {
            ConfigurationManager.Instance.OnConfigurationChanged += OnSettingsChanged;
            _currentConfig = ConfigurationManager.Instance.CurrentConfig.RecognitionSettings;
        }
        else _currentConfig = new RecognitionConfig();

        InitializeEngine();

        var existingCameras = FindObjectsByType<VirtualCameraSource>(FindObjectsSortMode.None);
        foreach (var cam in existingCameras) OnCameraInitialized(cam);
    }

    private void InitializeEngine()
    {
        // ИСПРАВЛЕНО: Строгое использование Фабрики!
        if (_currentConfig.EngineFactory != null)
        {
            inferenceEngine = _currentConfig.EngineFactory.CreateEngine(_currentConfig, classMapProvider);
        }
        else
        {
            Debug.LogError("RecognizeManager: Фабрика (EngineFactory) не назначена в SystemConfigurationSO! Запуск распознавания невозможен.");
        }
    }

    private void OnSettingsChanged(SystemConfigurationSO config)
    {
        _currentConfig = config.RecognitionSettings;
        if (inferenceEngine != null) inferenceEngine.UpdateConfig(_currentConfig);
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

    private void OnCameraRemoved(int cameraId)
    {
        _cameraNextDetectionTimes.Remove(cameraId);
        _activeCameras.RemoveAll(c => c == null || c.CameraId == cameraId);
    }

    void Update()
    {
        if (inferenceEngine == null || _currentConfig == null) return;

        for (int i = _activeCameras.Count - 1; i >= 0; i--)
        {
            var cam = _activeCameras[i];
            if (cam == null) { _activeCameras.RemoveAt(i); continue; }

            if (Time.time >= _cameraNextDetectionTimes[cam.CameraId] && cam.OutputTexture != null)
            {
                _cameraNextDetectionTimes[cam.CameraId] = Time.time + _currentConfig.DetectionInterval;
                if (!_processQueue.Contains(cam) && _processQueue.Count < 3) _processQueue.Enqueue(cam);
            }
        }

        if (!_isProcessing && _processQueue.Count > 0) _ = ProcessQueueAsync();
    }

    private async Task ProcessQueueAsync()
    {
        _isProcessing = true;
        while (_processQueue.Count > 0)
        {
            var cam = _processQueue.Dequeue();
            if (cam == null || cam.OutputTexture == null) continue;

            try
            {
                List<BoundingBox> foundBoxes = await inferenceEngine.RunInferenceAsync(cam.OutputTexture);
                if (cam == null) continue;

                DetectionResult result = new DetectionResult
                {
                    CameraId = cam.CameraId,
                    FrameWidth = _currentConfig.InputWidth,
                    FrameHeight = _currentConfig.InputHeight,
                    Boxes = foundBoxes
                };

                onCameraDetectionsCompleted?.Invoke(result);
            }
            catch (Exception ex) { Debug.LogWarning($"Ошибка детекции: {ex.Message}"); }
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
        if (ConfigurationManager.Instance != null)
            ConfigurationManager.Instance.OnConfigurationChanged -= OnSettingsChanged;
            
        inferenceEngine?.Dispose();
    }
}