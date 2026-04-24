using System;
using Surveillance.Settings;
using UnityEngine;

namespace Surveillance.Cameras
{
    [RequireComponent(typeof(Camera))]
    public sealed class VirtualCameraSource : MonoBehaviour
    {
        public int CameraId;

        // Текущие параметры (теперь они заполняются Менеджером настроек)
        private int width = 640;
        private int height = 360;
        private int depthBits = 24;
        private RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
        private int targetCaptureFps = 10;
        private bool startStreaming = true;
        
        private Camera _sourceCamera;
        private RenderTexture _renderTexture;

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
        
        private bool _isInitialized;
        private bool _isStreaming;
        private float _nextCaptureTime;
        
        
        private void Awake()
        {
            if (_sourceCamera == null)
                _sourceCamera = GetComponent<Camera>();
        }

        private void Start() => Initialize();

        private void Update()
        {
            if (!_isInitialized || !_isStreaming) return;
            if (Time.unscaledTime < _nextCaptureTime) return;

            float interval = 1f / Mathf.Max(1, GetTargetCaptureFps());
            _nextCaptureTime = Time.unscaledTime + interval;

            CaptureFrameNow();
        }

        private void OnDestroy() => ReleaseRenderTexture();

        public void Initialize()
        {
            if (_isInitialized) return;

            if (_sourceCamera == null) _sourceCamera = GetComponent<Camera>();
            
            CreateRenderTexture();
            _sourceCamera.targetTexture = _renderTexture;

            _sourceCamera.enabled = false;
            _isStreaming = startStreaming;
            _nextCaptureTime = Time.unscaledTime;
            _isInitialized = true;
        }
        
        public void CaptureFrameNow()
        {
            if (!_isInitialized || _sourceCamera == null || _renderTexture == null) return;
            _sourceCamera.Render();
        }
        
        public int GetTargetCaptureFps() => Mathf.Max(1, targetCaptureFps);

        // НОВЫЙ МЕТОД: Применение настроек из Центрального Модуля
        public void ApplyConfig(CameraConfig config)
        {
            if (config == null || _sourceCamera == null) return;

            width = config.RenderWidth;
            height = config.RenderHeight;
            depthBits = config.DepthBits;
            renderTextureFormat = config.Format;
            targetCaptureFps = config.TargetFps;
            startStreaming = config.StartStreaming;

            _sourceCamera.fieldOfView = config.FieldOfView;
            _sourceCamera.nearClipPlane = config.NearClipPlane;
            _sourceCamera.farClipPlane = config.FarClipPlane;
            _sourceCamera.clearFlags = config.ClearFlags;
            _sourceCamera.backgroundColor = config.BackgroundColor;
            _sourceCamera.allowHDR = config.AllowHdr;
            _sourceCamera.allowMSAA = config.AllowMsaa;

            // Пересоздаем текстуру, если изменилось разрешение
            if (_isInitialized)
            {
                CreateRenderTexture();
                _sourceCamera.targetTexture = _renderTexture;
            }
        }

        private void CreateRenderTexture()
        {
            ReleaseRenderTexture();
            _renderTexture = new RenderTexture(width, height, depthBits, renderTextureFormat)
            {
                name = $"RT_{CameraId}", useMipMap = false, autoGenerateMips = false
            };
            _renderTexture.Create();
        }

        private void ReleaseRenderTexture()
        {
            if (_renderTexture == null) return;
            if (_renderTexture.IsCreated()) _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }
    }
}