using Surveillance.Cameras;
using Surveillance.Settings; // <-- Добавлено
using UnityEngine;

public class MonitorSource : MonoBehaviour
{
    [SerializeField] private int monitorID;
    public int targetCameraId;
    
    // Текстура заглушки осталась в инспекторе, т.к. файлы ассетов не сохраняются в JSON
    [SerializeField] private Texture fallbackTexture;
    
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int materialIndex = 0;
    [SerializeField] private string texturePropertyName = "_BaseMap";

    private VirtualCameraManager _cameraManager;
    private VirtualCameraSource _boundCamera;
    private DisplayConfig _displayConfig;
    
    private MaterialPropertyBlock _propertyBlock;
    private int _texturePropertyId;
    private Texture _lastShownTexture;
    private bool _isStarted;

    public int MonitorID { get => monitorID; set { monitorID = value; ApplySettings(); } }
    public int TargetCameraId { get => targetCameraId; set { targetCameraId = value; ApplySettings(); } }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _texturePropertyId = Shader.PropertyToID(texturePropertyName);
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        _cameraManager = FindFirstObjectByType<VirtualCameraManager>();
        BindCamera();
        _isStarted = true;

        if (ConfigurationManager.Instance != null)
        {
            _displayConfig = ConfigurationManager.Instance.CurrentConfig.DisplaySettings;
        }

        ApplySettings();
    }

    public void ApplyConfig(DisplayConfig config)
    {
        _displayConfig = config;
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (!_isStarted) return; 
        BindCamera();
    }

    private void BindCamera()
    {
        if (_cameraManager == null)
        {
            ShowFallback();
            return;
        }

        _cameraManager.cameraInitializedEvent -= OnCameraRegistered;
        _cameraManager.cameraInitializedEvent += OnCameraRegistered;
        _boundCamera = _cameraManager.GetVirtualCamera(targetCameraId);
        Debug.Log(_boundCamera);

        if (_boundCamera != null) UpdateCameraTexture();
        else ShowFallback();
    }

    private void UpdateCameraTexture()
    {
        if (_boundCamera == null)
        {
            if (_displayConfig != null && _displayConfig.AutoRebind) BindCamera();
            else ShowFallback();
            return;
        }

        Texture texture = _boundCamera.OutputTexture;
        if (texture == null) ShowFallback();
        else ShowTexture(texture);
    }

    private void OnCameraRegistered(VirtualCameraSource source)
    {
        if (source == null || source.CameraId != targetCameraId) return;
        _boundCamera = source;
        UpdateCameraTexture();
    }

    private void ShowFallback()
    {
        if (_displayConfig != null && !_displayConfig.ShowFallbackWhenSourceMissing) return;
        if (fallbackTexture != null) ShowTexture(fallbackTexture);
        Debug.Log("adsdasda");
    }

    private void ShowTexture(Texture texture)
    {
        if (targetRenderer == null || texture == null) return;
        if (_lastShownTexture == texture) return;

        targetRenderer.GetPropertyBlock(_propertyBlock, materialIndex);
        _propertyBlock.SetTexture(_texturePropertyId, texture);
        targetRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
        _lastShownTexture = texture;
    }
}