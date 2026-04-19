using UnityEngine;

namespace Surveillance.Cameras
{
    [RequireComponent(typeof(Camera))]
    public sealed class VirtualCameraSource : MonoBehaviour
    {
        public int CameraId;

        [Header("Настройки")]

        [Header("Текстура рендера")]
        [Min(64)][SerializeField] private int width = 640;
        [Min(64)][SerializeField] private int height = 360;
        [Min(0)][SerializeField] private int depthBits = 24;
        [SerializeField]private RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;

        [Header("захват")]
        [Min(1)][SerializeField] private int targetCaptureFps = 10;
        public bool startStreaming = true;

        [Header("Параметры камеры")]
        [Range(10f, 120f)][SerializeField] private float fieldOfView = 60f;
        [Min(0.01f)][SerializeField] private float nearClipPlane = 0.1f;
        [Min(1f)][SerializeField] private float farClipPlane = 1000f;
        [SerializeField]private CameraClearFlags clearFlags = CameraClearFlags.Skybox;
        [SerializeField]private Color backgroundColor = Color.black;
        [SerializeField]private bool allowHdr = false;
        [SerializeField]private bool allowMsaa = false;
        
        private Camera _sourceCamera;

        private RenderTexture _renderTexture;
        public RenderTexture OutputTexture => _renderTexture;
        private bool _isInitialized;
        private bool _isStreaming;
        private float _nextCaptureTime;
        
        private void Awake()
        {
            if (_sourceCamera == null)
                _sourceCamera = GetComponent<Camera>();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_isInitialized || !_isStreaming)
                return;

            if (Time.unscaledTime < _nextCaptureTime)
                return;

            float interval = 1f / Mathf.Max(1, GetTargetCaptureFps());
            _nextCaptureTime = Time.unscaledTime + interval;

            CaptureFrameNow();
        }
        

        private void OnDestroy()
        {
            Destroy();

        }
        

        private void Destroy()
        {
            ReleaseRenderTexture();
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_sourceCamera == null)
                _sourceCamera = GetComponent<Camera>();
            
            CreateRenderTexture();
            
            _sourceCamera.enabled = false;
            _sourceCamera.targetTexture = _renderTexture;

            _isStreaming = startStreaming;
            _nextCaptureTime = Time.unscaledTime;
            _isInitialized = true;
        }
        
        public void CaptureFrameNow()
        {
            if (!_isInitialized || _sourceCamera == null || _renderTexture == null)
                return;

            _sourceCamera.Render();
        }
        
        public int GetTargetCaptureFps()
        {
            return  Mathf.Max(1, targetCaptureFps);
        }

        public void ApplyProfile(CameraCaptureProfileSO profile)
        {
            if (profile == null) return;

        
            width = profile.width;
            height = profile.height;
            depthBits = profile.depthBits;
            renderTextureFormat = profile.renderTextureFormat;

            targetCaptureFps = profile.targetCaptureFps;
            startStreaming = profile.startStreaming;

            fieldOfView = profile.fieldOfView;
            nearClipPlane = profile.nearClipPlane;
            farClipPlane = profile.farClipPlane;
            clearFlags = profile.clearFlags;
            backgroundColor = profile.backgroundColor;
            allowHdr = profile.allowHdr;
            allowMsaa = profile.allowMsaa;
            
            ApplyCameraSettings();
        }
        
        private void ApplyCameraSettings()
        {
            if (_sourceCamera == null)
                _sourceCamera = GetComponent<Camera>();

            if (_sourceCamera == null)
                return;

            _sourceCamera.fieldOfView = fieldOfView;
            _sourceCamera.nearClipPlane = nearClipPlane;
            _sourceCamera.farClipPlane = farClipPlane;
            _sourceCamera.clearFlags = clearFlags;
            _sourceCamera.backgroundColor = backgroundColor;
            _sourceCamera.allowHDR = allowHdr;
            _sourceCamera.allowMSAA = allowMsaa;
        }

        private void CreateRenderTexture()
        {
            ReleaseRenderTexture();
            
            _renderTexture = new RenderTexture(width, height, depthBits, renderTextureFormat)
            {
                name = $"RT_{CameraId}",
                useMipMap = false,
                autoGenerateMips = false
            };

            _renderTexture.Create();
        }

        private void ReleaseRenderTexture()
        {
            if (_renderTexture == null)
                return;

            if (_renderTexture.IsCreated())
                _renderTexture.Release();

            Destroy(_renderTexture);
            _renderTexture = null;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_sourceCamera == null)
                _sourceCamera = GetComponent<Camera>();


        }
#endif
    }
}