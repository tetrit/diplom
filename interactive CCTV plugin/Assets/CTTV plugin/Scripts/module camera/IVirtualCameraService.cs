using System;
using System.Collections.Generic;

namespace Surveillance.Cameras
{
    public interface IVirtualCameraService
    {
        IReadOnlyList<VirtualCameraSource> Cameras { get; }

        event Action<VirtualCameraSource> CameraRegistered;
        event Action<VirtualCameraSource> CameraUnregistered;
        event Action<VirtualCameraFrame> FrameProduced;

        void Register(VirtualCameraSource source);
        void Unregister(VirtualCameraSource source);

        bool TryGetCamera(string cameraId, out VirtualCameraSource source);
    }
}