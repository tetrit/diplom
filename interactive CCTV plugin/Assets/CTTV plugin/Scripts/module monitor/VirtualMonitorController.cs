using UnityEngine;
using Surveillance.Cameras;

namespace Surveillance.Monitors
{
    public sealed class VirtualMonitorController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private VirtualMonitorProfileSO profile;

        [Header("Scene refs")]
        [SerializeField] private VirtualMonitorView view;
        [SerializeField] private VirtualMonitorExternalInput externalInput;

        private IVirtualCameraService _cameraService;
        private VirtualCameraSource _boundCamera;
        private Texture _lastShownTexture;
        private bool _isStarted;

        private void Awake()
        {
            if (view == null)
                view = GetComponent<VirtualMonitorView>();
        }

        private void Start()
        {
            _isStarted = true;

            if (profile == null)
            {
                Debug.LogWarning($"[{name}] VirtualMonitorProfileSO is not assigned.");
                ShowFallback();
                return;
            }

            if (view == null)
            {
                Debug.LogError($"[{name}] VirtualMonitorView is not assigned.");
                return;
            }

            if (!profile.startEnabled)
            {
                ShowFallback();
                return;
            }

            if (profile.sourceMode == VirtualMonitorSourceMode.CameraStream)
            {
                BindCameraMode();
            }
            else
            {
                BindExternalMode();
            }
        }

        private void Update()
        {
            if (!_isStarted || profile == null)
                return;

            if (profile.sourceMode == VirtualMonitorSourceMode.CameraStream)
            {
                UpdateCameraTexture();
            }
            else
            {
                UpdateExternalTexture();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromCameraService();

            if (externalInput != null)
                externalInput.TextureChanged -= OnExternalTextureChanged;
        }

        public void SetCameraId(string cameraId)
        {
            if (profile == null)
                return;

            profile.cameraId = cameraId;

            if (profile.sourceMode == VirtualMonitorSourceMode.CameraStream && _isStarted)
                RebindCamera();
        }

        public void SetSourceMode(VirtualMonitorSourceMode sourceMode)
        {
            if (profile == null)
                return;

            if (profile.sourceMode == sourceMode)
                return;

            profile.sourceMode = sourceMode;

            if (!_isStarted)
                return;

            if (sourceMode == VirtualMonitorSourceMode.CameraStream)
            {
                if (externalInput != null)
                    externalInput.TextureChanged -= OnExternalTextureChanged;

                BindCameraMode();
            }
            else
            {
                UnsubscribeFromCameraService();
                _boundCamera = null;
                BindExternalMode();
            }
        }

        public void RefreshNow()
        {
            if (profile == null)
                return;

            if (profile.sourceMode == VirtualMonitorSourceMode.CameraStream)
                UpdateCameraTexture();
            else
                UpdateExternalTexture();
        }

        private void BindCameraMode()
        {
            if (!ServiceLocator.TryGet(out _cameraService))
            {
                Debug.LogWarning($"[{name}] IVirtualCameraService is not registered.");
                ShowFallback();
                return;
            }

            _cameraService.CameraRegistered += OnCameraRegistered;
            _cameraService.CameraUnregistered += OnCameraUnregistered;

            RebindCamera();
        }

        private void RebindCamera()
        {
            _boundCamera = null;
            _lastShownTexture = null;

            if (_cameraService == null || profile == null)
            {
                ShowFallback();
                return;
            }

            if (_cameraService.TryGetCamera(profile.cameraId, out VirtualCameraSource camera))
            {
                _boundCamera = camera;
                UpdateCameraTexture();
                return;
            }

            ShowFallback();
        }

        private void BindExternalMode()
        {
            if (externalInput == null)
            {
                Debug.LogWarning($"[{name}] ExternalInput mode selected, but VirtualMonitorExternalInput is not assigned.");
                ShowFallback();
                return;
            }

            externalInput.TextureChanged -= OnExternalTextureChanged;
            externalInput.TextureChanged += OnExternalTextureChanged;

            UpdateExternalTexture();
        }

        private void UpdateCameraTexture()
        {
            if (_boundCamera == null)
            {
                if (profile != null && profile.autoRebind)
                    RebindCamera();
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

            if (_lastShownTexture != texture)
            {
                view.Show(texture);
                _lastShownTexture = texture;
            }
        }

        private void UpdateExternalTexture()
        {
            if (externalInput == null)
            {
                ShowFallback();
                return;
            }

            Texture texture = externalInput.CurrentTexture;

            if (texture == null)
            {
                ShowFallback();
                return;
            }

            if (_lastShownTexture != texture)
            {
                view.Show(texture);
                _lastShownTexture = texture;
            }
        }

        private void OnExternalTextureChanged(Texture texture)
        {
            if (texture == null)
            {
                ShowFallback();
                return;
            }

            view.Show(texture);
            _lastShownTexture = texture;
        }

        private void OnCameraRegistered(VirtualCameraSource source)
        {
            if (profile == null || source == null)
                return;

            if (profile.sourceMode != VirtualMonitorSourceMode.CameraStream)
                return;

            if (source.CameraId != profile.cameraId)
                return;

            _boundCamera = source;
            UpdateCameraTexture();
        }

        private void OnCameraUnregistered(VirtualCameraSource source)
        {
            if (source == null || _boundCamera == null)
                return;

            if (source != _boundCamera)
                return;

            _boundCamera = null;
            _lastShownTexture = null;
            ShowFallback();
        }

        private void UnsubscribeFromCameraService()
        {
            if (_cameraService == null)
                return;

            _cameraService.CameraRegistered -= OnCameraRegistered;
            _cameraService.CameraUnregistered -= OnCameraUnregistered;
            _cameraService = null;
        }

        private void ShowFallback()
        {
            _lastShownTexture = null;

            if (profile != null && !profile.showFallbackWhenSourceMissing)
                return;

            if (view != null)
                view.ShowFallback();
        }
    }
}