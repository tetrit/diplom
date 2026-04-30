using System;
using Surveillance.Settings;
using UnityEngine;

namespace Surveillance.Cameras
{[RequireComponent(typeof(Camera))]
    public abstract class VirtualCameraSource : MonoBehaviour // Убрали sealed, добавили abstract
    {
        public int CameraId;

        // Поля теперь protected, чтобы наследники могли их читать/изменять,
        // но они остаются закрытыми для внешних классов.
        protected int width = 640;
        protected int height = 360;
        protected int depthBits = 24;
        protected RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
        protected int targetCaptureFps = 10;
        protected bool startStreaming = true;
        
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
        
        // Unity-методы сделаны protected virtual
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

        // Основные методы сделаны virtual, если наследник захочет изменить их логику
        public virtual void Initialize()
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
        
        public virtual void CaptureFrameNow()
        {
            if (!_isInitialized || _sourceCamera == null || _renderTexture == null) return;
            _sourceCamera.Render();
        }
        
        public int GetTargetCaptureFps() => Mathf.Max(1, targetCaptureFps);

        public virtual void ApplyConfig(CameraConfig config)
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
            _renderTexture = new RenderTexture(width, height, depthBits, renderTextureFormat)
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