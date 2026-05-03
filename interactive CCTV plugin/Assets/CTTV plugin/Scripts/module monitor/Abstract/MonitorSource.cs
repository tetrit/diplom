using Surveillance.Cameras;
using Surveillance.Settings;
using UnityEngine;

public abstract class MonitorSource : MonoBehaviour
{
    [SerializeField] protected int monitorID;
    public int targetCameraId;
    
    [SerializeField] protected Texture fallbackTexture;
    
    [SerializeField] protected Renderer targetRenderer;
    [SerializeField] protected int materialIndex = 0;
    [SerializeField] protected string texturePropertyName = "_BaseMap";

    [Header("Источники настроек")][Tooltip("Включено - получение настроек по умолчанию из SystemConfiguration. Выключено - применение индивидуальных настроек.")]
    [SerializeField] protected bool useGlobalConfig = true;

    [Header("Индивидуальные настройки отображения")]
    [SerializeField] protected bool showFallbackWhenSourceMissing = true;
    [SerializeField] protected bool autoRebind = true;[SerializeField] protected Color boundingBoxColor = Color.green;
    [SerializeField] protected int maxBoxesOnScreen = 30;

    protected VirtualCameraManager _cameraManager;
    protected VirtualCameraSource _boundCamera;
    
    protected MaterialPropertyBlock _propertyBlock;
    protected int _texturePropertyId;
    protected Texture _lastShownTexture;
    protected bool _isStarted;

    public int MonitorID { get => monitorID; set { monitorID = value; ApplySettings(); } }
    public int TargetCameraId { get => targetCameraId; set { targetCameraId = value; ApplySettings(); } }

    public Color BoundingBoxColor => boundingBoxColor;
    public int MaxBoxesOnScreen => maxBoxesOnScreen;

    protected virtual void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _texturePropertyId = Shader.PropertyToID(texturePropertyName);
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
    }

    protected virtual void Start()
    {
        _cameraManager = FindFirstObjectByType<VirtualCameraManager>();

        if (useGlobalConfig && ConfigurationManager.Instance != null)
        {
            ApplyConfig(ConfigurationManager.Instance.CurrentConfig.DisplaySettings);
        }

        BindCamera();
        _isStarted = true;
        
        if (_boundCamera != null)
            _boundCamera.CameraReloaded += BindCamera;

        ApplySettings();
    }

    public virtual void ApplyConfig(DisplayConfig config)
    {
        if (config != null && useGlobalConfig)
        {
            showFallbackWhenSourceMissing = config.ShowFallbackWhenSourceMissing;
            autoRebind = config.AutoRebind;
            boundingBoxColor = config.BoundingBoxColor;
            maxBoxesOnScreen = config.MaxBoxesOnScreen;
        }
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
        
        if (_boundCamera != null)
        {
            _boundCamera.CameraReloaded -= BindCamera;
        }

        _boundCamera = _cameraManager.GetVirtualCamera(targetCameraId);

        if (_boundCamera != null) 
        {
            _boundCamera.CameraReloaded += BindCamera;
            UpdateCameraTexture();
        }
        else ShowFallback();
    }

    protected virtual void UpdateCameraTexture()
    {
        if (_boundCamera == null)
        {
            if (autoRebind) BindCamera();
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
        if (_boundCamera != null) _boundCamera.CameraReloaded += BindCamera;
        UpdateCameraTexture();
    }

    protected virtual void ShowFallback()
    {
        if (!showFallbackWhenSourceMissing) return;
        if (fallbackTexture != null) ShowTexture(fallbackTexture);
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

    protected virtual void OnDestroy()
    {
        if (_boundCamera != null) _boundCamera.CameraReloaded -= BindCamera;
        if (_cameraManager != null) _cameraManager.cameraInitializedEvent -= OnCameraRegistered;
    }
}