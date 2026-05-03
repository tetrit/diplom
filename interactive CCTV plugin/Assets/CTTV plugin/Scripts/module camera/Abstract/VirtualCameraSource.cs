using System;
using Surveillance.Settings;
using UnityEngine;

namespace Surveillance.Cameras
{[RequireComponent(typeof(Camera))]
    public abstract class VirtualCameraSource : MonoBehaviour 
    {
        public int CameraId;

        [Header("Источники настроек")][Tooltip("Включено - получение настроек по умолчанию из SystemConfiguration. Если выключено - применение индивидуальных настроек.")]
        [SerializeField] protected bool useGlobalConfig = true;

        [Header("Индивидуальные настройки камеры")]
        [SerializeField][Min(100)] protected int renderWidth = 640;
        [SerializeField][Min(100)] protected int renderHeight = 360;
        [SerializeField][Min(1)] protected int depthBits = 24;
        [SerializeField] protected RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
        [SerializeField] protected int targetCaptureFps = 10;[SerializeField] protected bool startStreaming = true;

        [SerializeField][Range(10, 180)] protected float fieldOfView = 60f;
        [SerializeField][Min(0.1f)] protected float nearClipPlane = 0.1f;
        [SerializeField][Min(1f)] protected float farClipPlane = 1000f;
        [SerializeField] protected CameraClearFlags clearFlags = CameraClearFlags.Skybox;
        [SerializeField] protected Color backgroundColor = Color.black;
        [SerializeField] protected bool allowHdr = false;
        [SerializeField] protected bool allowMsaa = false;
        
        protected Camera _sourceCamera;
        protected RenderTexture _renderTexture;
        
        protected bool _isInitialized;
        protected bool _isStreaming;
        protected float _nextCaptureTime;

        public event Action CameraReloaded;

        public RenderTexture OutputTexture
        {
            get
            {
                if (_renderTexture == null)
                {
                    Initialize();
                }
                return _renderTexture;
            }
        }
        
        protected virtual void Awake()
        {
            if (_sourceCamera == null)
                _sourceCamera = GetComponent<Camera>();
        }

        protected virtual void Start() => Initialize();

        protected virtual void Update()
        {
            if (!_isInitialized || !_isStreaming) return;
            if (Time.unscaledTime < _nextCaptureTime) return;

            float interval = 1f / Mathf.Max(1, GetTargetCaptureFps());
            _nextCaptureTime = Time.unscaledTime + interval;

            CaptureFrameNow();
        }

        protected virtual void OnDestroy() => ReleaseRenderTexture();

        public virtual void Initialize()
        {
            if (_isInitialized) return;

            if (_sourceCamera == null) _sourceCamera = GetComponent<Camera>();
            
            if (useGlobalConfig && ConfigurationManager.Instance != null)
            {
                ApplyConfig(ConfigurationManager.Instance.CurrentConfig.CameraSettings);
            }
            else
            {
                ApplyCurrentSettings();
            }
            
            CreateRenderTexture();
            _sourceCamera.targetTexture = _renderTexture;

            _sourceCamera.enabled = false;
            _isStreaming = startStreaming;
            _nextCaptureTime = Time.unscaledTime;
            _isInitialized = true;
        }
        
        public virtual void CaptureFrameNow()
        {
            if (!_isInitialized || _sourceCamera == null || _renderTexture == null) return;
            _sourceCamera.Render();
        }
        
        public int GetTargetCaptureFps() => Mathf.Max(1, targetCaptureFps);

        public virtual void ApplyConfig(CameraConfig config)
        {
            if (config == null || _sourceCamera == null) return;

            if (useGlobalConfig)
            {
                renderWidth = config.RenderWidth;
                renderHeight = config.RenderHeight;
                depthBits = config.DepthBits;
                renderTextureFormat = config.Format;
                targetCaptureFps = config.TargetFps;
                startStreaming = config.StartStreaming;

                fieldOfView = config.FieldOfView;
                nearClipPlane = config.NearClipPlane;
                farClipPlane = config.FarClipPlane;
                clearFlags = config.ClearFlags;
                backgroundColor = config.BackgroundColor;
                allowHdr = config.AllowHdr;
                allowMsaa = config.AllowMsaa;
            }

            ApplyCurrentSettings();
        }

        public virtual void ApplyCurrentSettings()
        {
            if (_sourceCamera == null) return;

            _sourceCamera.fieldOfView = fieldOfView;
            _sourceCamera.nearClipPlane = nearClipPlane;
            _sourceCamera.farClipPlane = farClipPlane;
            _sourceCamera.clearFlags = clearFlags;
            _sourceCamera.backgroundColor = backgroundColor;
            _sourceCamera.allowHDR = allowHdr;
            _sourceCamera.allowMSAA = allowMsaa;

            _isStreaming = startStreaming;

            if (_isInitialized)
            {
                CreateRenderTexture();
                _sourceCamera.targetTexture = _renderTexture;
            }
            CameraReloaded?.Invoke();
        }

        protected virtual void CreateRenderTexture()
        {
            ReleaseRenderTexture();
            _renderTexture = new RenderTexture(renderWidth, renderHeight, depthBits, renderTextureFormat)
            {
                name = $"RT_{CameraId}", useMipMap = false, autoGenerateMips = false
            };
            _renderTexture.Create();
        }

        protected virtual void ReleaseRenderTexture()
        {
            if (_renderTexture == null) return;
            if (_renderTexture.IsCreated()) _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }
    }
}