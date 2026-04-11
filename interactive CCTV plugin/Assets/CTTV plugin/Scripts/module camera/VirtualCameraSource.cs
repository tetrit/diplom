using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Surveillance.Cameras
{
    [RequireComponent(typeof(Camera))]
    public sealed class VirtualCameraSource : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private int cameraId = 1;

        public int CameraId
        {
            get
            {
                return CameraId = cameraId;
            }
            set
            {
                cameraId = value;
            }
        }

        [Header("Config")]
        [SerializeField] private CameraCaptureProfileSO profile;

        public int fps
        {
            get
            {
                return profile.targetCaptureFps;
            }
        }
        


        [Header("Optional refs")]
        [SerializeField] private Camera sourceCamera;

        private RenderTexture _renderTexture;
        public RenderTexture OutputTexture => _renderTexture;
        private bool _isInitialized;
        private bool _isStreaming;
        private float _nextCaptureTime;
        private long _frameIndex;


        //public Camera UnityCamera => sourceCamera;

        public event Action<VirtualCameraFrame> FrameProduced;
        
        //TODO: событие перекинуть в менеджер камер
        public event Action<VirtualCameraParamForPredict>  ProfileProduced;
        
        

        private void Awake()
        {
            if (sourceCamera == null)
                sourceCamera = GetComponent<Camera>();
        }

        private void Start()
        {
            Initialize();



            VirtualCameraParamForPredict paramForPredict = new VirtualCameraParamForPredict(profile.width, profile.height, profile.targetCaptureFps, _renderTexture);
            ProfileProduced?.Invoke(paramForPredict);
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

            if (sourceCamera == null)
                sourceCamera = GetComponent<Camera>();

            ApplyProfile();
            CreateRenderTexture();
            
            sourceCamera.enabled = false;
            sourceCamera.targetTexture = _renderTexture;

            _isStreaming = profile == null || profile.startStreaming;
            _nextCaptureTime = Time.unscaledTime;
            _isInitialized = true;
        }

        public void SetStreaming(bool value)
        {
            _isStreaming = value;
            _nextCaptureTime = Time.unscaledTime;
        }

        public void CaptureFrameNow()
        {
            if (!_isInitialized || sourceCamera == null || _renderTexture == null)
                return;

            sourceCamera.Render();

            _frameIndex++;

            VirtualCameraFrame frame = new(
                cameraId,
                _frameIndex,
                Time.unscaledTime,
                _renderTexture);

            FrameProduced?.Invoke(frame);
        }

        public void RequestCpuFrame(Action<VirtualCameraCpuFrame> onReady)
        {
            if (onReady == null || !_isInitialized || _renderTexture == null)
                return;

            StartCoroutine(ReadbackCpuFrameCoroutine(onReady));
        }

        public void RebuildFromProfile()
        {
            ApplyProfile();
            CreateRenderTexture();

            if (sourceCamera != null)
                sourceCamera.targetTexture = _renderTexture;

            _nextCaptureTime = Time.unscaledTime;
        }

        public int GetTargetCaptureFps()
        {
            return profile != null ? Mathf.Max(1, profile.targetCaptureFps) : 10;
        }

        private void ApplyProfile()
        {
            if (sourceCamera == null)
                return;

            if (profile == null)
                return;

            sourceCamera.fieldOfView = profile.fieldOfView;
            sourceCamera.nearClipPlane = profile.nearClipPlane;
            sourceCamera.farClipPlane = profile.farClipPlane;
            sourceCamera.clearFlags = profile.clearFlags;
            sourceCamera.backgroundColor = profile.backgroundColor;
            sourceCamera.allowHDR = profile.allowHdr;
            sourceCamera.allowMSAA = profile.allowMsaa;
        }

        private void CreateRenderTexture()
        {
            ReleaseRenderTexture();

            int width = profile != null ? profile.width : 640;
            int height = profile != null ? profile.height : 360;
            int depthBits = profile != null ? profile.depthBits : 24;
            RenderTextureFormat format = profile != null
                ? profile.renderTextureFormat
                : RenderTextureFormat.ARGB32;

            _renderTexture = new RenderTexture(width, height, depthBits, format)
            {
                name = $"RT_{cameraId}",
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
        

        private IEnumerator ReadbackCpuFrameCoroutine(Action<VirtualCameraCpuFrame> onReady)
        {
            // Актуализируем изображение перед чтением.
            sourceCamera.Render();

#if UNITY_2018_2_OR_NEWER
            if (SystemInfo.supportsAsyncGPUReadback)
            {
                AsyncGPUReadbackRequest request =
                    AsyncGPUReadback.Request(_renderTexture, 0, TextureFormat.RGBA32);

                while (!request.done)
                    yield return null;

                if (!request.hasError)
                {
                    Texture2D texture = new(
                        _renderTexture.width,
                        _renderTexture.height,
                        TextureFormat.RGBA32,
                        false,
                        false);

                    texture.LoadRawTextureData(request.GetData<byte>());
                    texture.Apply(false, false);

                    onReady.Invoke(new VirtualCameraCpuFrame(
                        cameraId,
                        _frameIndex,
                        Time.unscaledTime,
                        texture));

                    yield break;
                }
            }
#endif

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = _renderTexture;

            Texture2D fallbackTexture = new(
                _renderTexture.width,
                _renderTexture.height,
                TextureFormat.RGBA32,
                false,
                false);

            fallbackTexture.ReadPixels(
                new Rect(0, 0, _renderTexture.width, _renderTexture.height),
                0,
                0,
                false);

            fallbackTexture.Apply(false, false);
            RenderTexture.active = previous;

            onReady.Invoke(new VirtualCameraCpuFrame(
                cameraId,
                _frameIndex,
                Time.unscaledTime,
                fallbackTexture));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sourceCamera == null)
                sourceCamera = GetComponent<Camera>();


        }
#endif
    }


}