using UnityEngine;
using Surveillance.Cameras;

namespace Surveillance.Monitors
{
    public sealed class VirtualMonitorController : MonoBehaviour
    {
        private VirtualMonitorProfileSO _profile;
        private int _targetCameraId;

        [SerializeField] private VirtualMonitorView _view;
        private VirtualCameraManager _virtualCameraManager;
        private VirtualCameraSource _boundCamera;
        private Texture _lastShownTexture;
        private bool _isStarted;

        public void Initialize(int cameraId, VirtualMonitorProfileSO profile)
        {
            _targetCameraId = cameraId;
            _profile = profile;
            
            if (_view == null)
                _view = GetComponentInChildren<VirtualMonitorView>();
            
            if (_virtualCameraManager == null)
                _virtualCameraManager = FindFirstObjectByType<VirtualCameraManager>();
            
            _isStarted = true;

            if (_profile == null || !_profile.startEnabled)
            {
                ShowFallback();
                return;
            }

            BindCamera();
        }

        private void Update()
        {
            if (!_isStarted || _profile == null)
                return;

            UpdateCameraTexture();
        }

        private void OnDestroy()
        {
            if (_virtualCameraManager != null)
                _virtualCameraManager.cameraInitializedEvent -= OnCameraRegistered;
        }

        private void BindCamera()
        {
            if (_virtualCameraManager == null)
            {
                ShowFallback();
                return;
            }

            _virtualCameraManager.cameraInitializedEvent -= OnCameraRegistered;
            _virtualCameraManager.cameraInitializedEvent += OnCameraRegistered;

            _boundCamera = _virtualCameraManager.GetVirtualCamera(_targetCameraId);

            if (_boundCamera != null)
                UpdateCameraTexture();
            else
                ShowFallback();
        }

        private void UpdateCameraTexture()
        {
            if (_boundCamera == null)
            {
                if (_profile != null && _profile.autoRebind)
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

            if (_lastShownTexture != texture)
            {
                if (_view != null) _view.Show(texture);
                _lastShownTexture = texture;
            }
        }

        private void OnCameraRegistered(VirtualCameraSource source)
        {
            if (_profile == null || source == null) return;
            if (source.CameraId != _targetCameraId) return;

            _boundCamera = source;
            UpdateCameraTexture();
        }

        private void ShowFallback()
        {
            _lastShownTexture = null;

            if (_profile != null && !_profile.showFallbackWhenSourceMissing)
                return;

            if (_view != null && _profile != null && _profile.fallbackTexture != null)
                _view.Show(_profile.fallbackTexture);
        }
    }
}