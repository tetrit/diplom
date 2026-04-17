using UnityEngine;
using Surveillance.Cameras;

namespace Surveillance.Monitors
{
    public sealed class VirtualMonitorController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private VirtualMonitorProfileSO profile;
        public VirtualMonitorProfileSO Profile
        {
            get { return profile; }
            set { profile = value; }
        }
        [SerializeField]private VirtualCameraManager virtualCameraManager;

        [Header("Per-monitor source settings")]
        [SerializeField] private int cameraId;
        public int CameraId
        {
            get { return cameraId; }
            set { cameraId = value; }
        }


        [Header("Scene refs")]
        [SerializeField] private VirtualMonitorView view;

        [SerializeField] private VirtualMonitorExternalInput externalInput;


        private VirtualCameraSource boundCamera;

        private Texture lastShownTexture;

        private bool isStarted;

        private void Awake()
        {
            if (view == null)
                view = GetComponent<VirtualMonitorView>();
            
            virtualCameraManager = FindObjectOfType<VirtualCameraManager>();
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
            if (virtualCameraManager != null)
                virtualCameraManager.cameraInitializedEvent -= OnCameraRegistered;

            if (externalInput != null)
                externalInput.TextureChanged -= OnExternalTextureChanged;
        }

        public void SetCameraId(int newCameraId)
        {
            cameraId = newCameraId;
            bindCamera();
        }

        private void BindCameraMode()
        {
            if (virtualCameraManager == null)
            {
                ShowFallback();
                return;
            }

            virtualCameraManager.cameraInitializedEvent -= OnCameraRegistered;
            virtualCameraManager.cameraInitializedEvent += OnCameraRegistered;

            bindCamera();
        }

        private void bindCamera()
        {
            boundCamera = null;
            lastShownTexture = null;

            if (virtualCameraManager == null || profile == null)
            {
                ShowFallback();
                return;
            }

            boundCamera = virtualCameraManager.GetVirtualCamera(cameraId);

            if (boundCamera != null)
                UpdateCameraTexture();
            else
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
                    bindCamera();
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
            Debug.Log(source.CameraId);
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