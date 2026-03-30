using System;
using System.Collections.Generic;
using UnityEngine;

namespace Surveillance.Cameras
{
    public sealed class VirtualCameraService : IVirtualCameraService
    {
        private readonly List<VirtualCameraSource> _cameras = new();
        private readonly Dictionary<string, VirtualCameraSource> _camerasById = new(StringComparer.Ordinal);

        public IReadOnlyList<VirtualCameraSource> Cameras => _cameras;

        public event Action<VirtualCameraSource> CameraRegistered;
        public event Action<VirtualCameraSource> CameraUnregistered;
        public event Action<VirtualCameraFrame> FrameProduced;

        public void Register(VirtualCameraSource source)
        {
            if (source == null)
                return;

            if (string.IsNullOrWhiteSpace(source.CameraId))
            {
                Debug.LogWarning("VirtualCameraSource has empty CameraId and will not be registered.");
                return;
            }

            if (_camerasById.ContainsKey(source.CameraId))
            {
                Debug.LogWarning($"Camera with id '{source.CameraId}' is already registered.");
                return;
            }

            _cameras.Add(source);
            _camerasById.Add(source.CameraId, source);
            source.FrameProduced += OnFrameProduced;

            CameraRegistered?.Invoke(source);
        }

        public void Unregister(VirtualCameraSource source)
        {
            if (source == null)
                return;

            if (_camerasById.Remove(source.CameraId))
            {
                _cameras.Remove(source);
                source.FrameProduced -= OnFrameProduced;
                CameraUnregistered?.Invoke(source);
            }
        }

        public bool TryGetCamera(string cameraId, out VirtualCameraSource source)
        {
            return _camerasById.TryGetValue(cameraId, out source);
        }

        private void OnFrameProduced(VirtualCameraFrame frame)
        {
            FrameProduced?.Invoke(frame);
        }
    }
}