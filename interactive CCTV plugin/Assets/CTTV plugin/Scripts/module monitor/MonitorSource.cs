using Surveillance.Cameras;
using Surveillance.Monitors;
using UnityEngine;

public class MonitorSource : MonoBehaviour
{
    [Header("Базовые настройки")]
    [SerializeField] private int monitorID;
    [SerializeField] private int targetCameraId;
    [SerializeField] private VirtualMonitorProfileSO profile;

    [Header("Настройки рендера (View)")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private int materialIndex = 0;
    [SerializeField] private string texturePropertyName = "_BaseMap"; // _BaseMap для URP, _MainTex для Standard

    private VirtualCameraManager _cameraManager;
    private VirtualCameraSource _boundCamera;
    
    private MaterialPropertyBlock _propertyBlock;
    private int _texturePropertyId;
    private Texture _lastShownTexture;
    private bool _isStarted;

    public int MonitorID
    {
        get => monitorID;
        set
        {
            monitorID = value;
            ApplySettings();
        }
    }

    public int TargetCameraId
    {
        get => targetCameraId;
        set
        {
            targetCameraId = value;
            ApplySettings();
        }
    }

    private void Awake()
    {
        // Инициализируем компоненты для рендера сразу
        _propertyBlock = new MaterialPropertyBlock();
        _texturePropertyId = Shader.PropertyToID(texturePropertyName);

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        _cameraManager = FindFirstObjectByType<VirtualCameraManager>();
        _isStarted = true;
        
        ApplySettings();
    }

    private void Update()
    {
        if (!_isStarted || profile == null) return;
        
        UpdateCameraTexture();
    }

    private void OnDestroy()
    {
        if (_cameraManager != null)
            _cameraManager.cameraInitializedEvent -= OnCameraRegistered;
    }

    public void ApplyProfile(VirtualMonitorProfileSO newProfile)
    {
        profile = newProfile;
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (!_isStarted) return; // Ждем Start(), если настройки меняются из Awake других скриптов

        if (profile == null || !profile.startEnabled)
        {
            ShowFallback();
            return;
        }

        BindCamera();
    }

    private void BindCamera()
    {
        if (_cameraManager == null)
        {
            ShowFallback();
            return;
        }

        // Переподписываемся на события, чтобы избежать дубликатов
        _cameraManager.cameraInitializedEvent -= OnCameraRegistered;
        _cameraManager.cameraInitializedEvent += OnCameraRegistered;

        _boundCamera = _cameraManager.GetVirtualCamera(targetCameraId);

        if (_boundCamera != null)
            UpdateCameraTexture();
        else
            ShowFallback();
    }

    private void UpdateCameraTexture()
    {
        if (_boundCamera == null)
        {
            if (profile != null && profile.autoRebind)
                BindCamera();
            else
                ShowFallback();
            return;
        }

        Texture texture = _boundCamera.OutputTexture;

        if (texture == null)
        {
            ShowFallback();
            return;
        }

        ShowTexture(texture);
    }

    private void OnCameraRegistered(VirtualCameraSource source)
    {
        if (profile == null || source == null) return;
        if (source.CameraId != targetCameraId) return;

        _boundCamera = source;
        UpdateCameraTexture();
    }

    private void ShowFallback()
    {
        if (profile != null && !profile.showFallbackWhenSourceMissing)
            return;

        if (profile != null && profile.fallbackTexture != null)
            ShowTexture(profile.fallbackTexture);
    }

    private void ShowTexture(Texture texture)
    {
        if (targetRenderer == null || texture == null) return;
        
        // Оптимизация: не обновляем материал, если текстура не изменилась
        if (_lastShownTexture == texture) return;

        targetRenderer.GetPropertyBlock(_propertyBlock, materialIndex);
        _propertyBlock.SetTexture(_texturePropertyId, texture);
        targetRenderer.SetPropertyBlock(_propertyBlock, materialIndex);

        _lastShownTexture = texture;
    }
}