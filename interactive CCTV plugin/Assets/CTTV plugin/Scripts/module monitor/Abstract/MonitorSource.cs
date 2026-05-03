using Surveillance.Cameras;
using Surveillance.Settings; // <-- Добавлено
using UnityEngine;

public abstract class MonitorSource : MonoBehaviour
{
    [SerializeField] protected int monitorID;
    public int targetCameraId;
    
    [SerializeField] protected Texture fallbackTexture;
    
    [SerializeField] protected Renderer targetRenderer;
    [SerializeField] protected int materialIndex = 0;
    [SerializeField] protected string texturePropertyName = "_BaseMap";

    protected VirtualCameraManager _cameraManager;
    protected VirtualCameraSource _boundCamera;
    protected DisplayConfig _displayConfig;
    
    protected MaterialPropertyBlock _propertyBlock;
    protected int _texturePropertyId;
    protected Texture _lastShownTexture;
    protected bool _isStarted;

    public int MonitorID { get => monitorID; set { monitorID = value; ApplySettings(); } }
    public int TargetCameraId { get => targetCameraId; set { targetCameraId = value; ApplySettings(); } }

    protected virtual void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _texturePropertyId = Shader.PropertyToID(texturePropertyName);
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
    }

    protected virtual void Start()
    {
        _cameraManager = FindFirstObjectByType<VirtualCameraManager>();
        BindCamera();
        _isStarted = true;
        _boundCamera.CameraReloaded += BindCamera;

        if (ConfigurationManager.Instance != null)
        {
            _displayConfig = ConfigurationManager.Instance.CurrentConfig.DisplaySettings;
        }

        ApplySettings();
    }

    public virtual void ApplyConfig(DisplayConfig config)
    {
        _displayConfig = config;
        ApplySettings();
    }

    public virtual void ApplySettings()
    {
        if (!_isStarted) return; 
        BindCamera();
    }

    protected virtual void BindCamera()
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

    protected virtual void UpdateCameraTexture()
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

    protected virtual void OnCameraRegistered(VirtualCameraSource source)
    {
        if (source == null || source.CameraId != targetCameraId) return;
        _boundCamera = source;
        UpdateCameraTexture();
    }

    protected virtual void ShowFallback()
    {
        if (_displayConfig != null && !_displayConfig.ShowFallbackWhenSourceMissing) return;
        if (fallbackTexture != null) ShowTexture(fallbackTexture);
        Debug.Log("adsdasda");
    }

    protected virtual void ShowTexture(Texture texture)
    {
        if (targetRenderer == null || texture == null) return;
        if (_lastShownTexture == texture) return;

        targetRenderer.GetPropertyBlock(_propertyBlock, materialIndex);
        _propertyBlock.SetTexture(_texturePropertyId, texture);
        targetRenderer.SetPropertyBlock(_propertyBlock, materialIndex);
        _lastShownTexture = texture;
    }
}