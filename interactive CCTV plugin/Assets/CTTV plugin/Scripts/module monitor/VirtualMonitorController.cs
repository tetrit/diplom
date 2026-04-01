using UnityEngine;
using Surveillance.Cameras;

namespace Surveillance.Monitors
{
    public sealed class VirtualMonitorController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private VirtualMonitorProfileSO profile;

        [Header("Per-monitor source settings")]
        [SerializeField] private string cameraId = "camera_01";

        [Header("Scene refs")]
        [SerializeField] private VirtualMonitorView view;
        [SerializeField] private VirtualMonitorExternalInput externalInput;

        private IVirtualCameraService cameraService;
        private VirtualCameraSource boundCamera;
        private Texture lastShownTexture;
        private bool isStarted;

        public string CameraId
        {
            get { return cameraId; }
        }

        private void Awake()
        {
            if (view == null)
                view = GetComponent<VirtualMonitorView>();
        }

        private void Start()
        {
            isStarted = true;

            if (profile == null)
            {
                Debug.LogWarning("[" + name + "] VirtualMonitorProfileSO is not assigned.");
                ShowFallback();
                return;
            }

            if (view == null)
            {
                Debug.LogError("[" + name + "] VirtualMonitorView is not assigned.");
                return;
            }

            if (!profile.startEnabled)
            {
                ShowFallback();
                return;
            }

            if (profile.sourceMode == VirtualMonitorSourceMode.CameraStream)
                BindCameraMode();
            else
                BindExternalMode();
        }

        private void Update()
        {
            if (!isStarted || profile == null)
                return;

            if (profile.sourceMode == VirtualMonitorSourceMode.CameraStream)
                UpdateCameraTexture();
            else
                UpdateExternalTexture();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCameraService();

            if (externalInput != null)
                externalInput.TextureChanged -= OnExternalTextureChanged;
        }

        public void SetCameraId(string newCameraId)
        {
            cameraId = newCameraId;
            RebindCamera();
        }

        private void BindCameraMode()
        {
            if (!ServiceLocator.TryGet<IVirtualCameraService>(out cameraService))
            {
                Debug.LogWarning("[" + name + "] IVirtualCameraService is not registered.");
                ShowFallback();
                return;
            }

            cameraService.CameraRegistered += OnCameraRegistered;
            cameraService.CameraUnregistered += OnCameraUnregistered;

            RebindCamera();
        }

        private void RebindCamera()
        {
            boundCamera = null;
            lastShownTexture = null;

            if (cameraService == null || profile == null)
            {
                ShowFallback();
                return;
            }

            VirtualCameraSource camera;
            if (cameraService.TryGetCamera(cameraId, out camera))
            {
                boundCamera = camera;
                UpdateCameraTexture();
                return;
            }

            ShowFallback();
        }

        private void BindExternalMode()
        {
            if (externalInput == null)
            {
                Debug.LogWarning("[" + name + "] ExternalInput mode selected, but VirtualMonitorExternalInput is not assigned.");
                ShowFallback();
                return;
            }

            externalInput.TextureChanged -= OnExternalTextureChanged;
            externalInput.TextureChanged += OnExternalTextureChanged;

            UpdateExternalTexture();
        }

        private void UpdateCameraTexture()
        {
            if (boundCamera == null)
            {
                if (profile != null && profile.autoRebind)
                    RebindCamera();
                else
                    ShowFallback();

                return;
            }

            Texture texture = boundCamera.OutputTexture;

            if (texture == null)
            {
                ShowFallback();
                return;
            }

            if (lastShownTexture != texture)
            {
                view.Show(texture);
                lastShownTexture = texture;
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

            if (lastShownTexture != texture)
            {
                view.Show(texture);
                lastShownTexture = texture;
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
            lastShownTexture = texture;
        }

        private void OnCameraRegistered(VirtualCameraSource source)
        {
            if (profile == null || source == null)
                return;

            if (profile.sourceMode != VirtualMonitorSourceMode.CameraStream)
                return;

            if (source.CameraId != cameraId)
                return;

            boundCamera = source;
            UpdateCameraTexture();
        }

        private void OnCameraUnregistered(VirtualCameraSource source)
        {
            if (source == null || boundCamera == null)
                return;

            if (source != boundCamera)
                return;

            boundCamera = null;
            lastShownTexture = null;
            ShowFallback();
        }

        private void UnsubscribeFromCameraService()
        {
            if (cameraService == null)
                return;

            cameraService.CameraRegistered -= OnCameraRegistered;
            cameraService.CameraUnregistered -= OnCameraUnregistered;
            cameraService = null;
        }

        private void ShowFallback()
        {
            lastShownTexture = null;

            if (profile != null && !profile.showFallbackWhenSourceMissing)
                return;

            if (view != null)
                view.ShowFallback();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (view == null)
                view = GetComponent<VirtualMonitorView>();
        }
#endif
    }
}