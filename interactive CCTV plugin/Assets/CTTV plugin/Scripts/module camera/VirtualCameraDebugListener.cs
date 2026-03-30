using UnityEngine;

namespace Surveillance.Cameras
{
    public sealed class VirtualCameraDebugListener : MonoBehaviour
    {
        private IVirtualCameraService _cameraService;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _cameraService))
            {
                Debug.LogWarning("IVirtualCameraService was not found.");
                return;
            }

            _cameraService.FrameProduced += OnFrameProduced;
        }

        private void OnDestroy()
        {
            if (_cameraService != null)
                _cameraService.FrameProduced -= OnFrameProduced;
        }

        private void OnFrameProduced(VirtualCameraFrame frame)
        {
            Debug.Log(
                $"[VirtualCamera] {frame.CameraId} frame={frame.FrameIndex} size={frame.Width}x{frame.Height}");
        }
    }
}