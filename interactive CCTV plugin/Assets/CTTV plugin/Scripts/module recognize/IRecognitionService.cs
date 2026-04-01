using System;
using System.Collections.Generic;

namespace Surveillance.Recognition
{
    public interface IRecognitionService
    {
        IReadOnlyList<YoloVirtualCameraDetector> Detectors { get; }

        event Action<YoloVirtualCameraDetector> DetectorRegistered;
        event Action<YoloVirtualCameraDetector> DetectorUnregistered;
        event Action<DetectionFrame> DetectionFrameProduced;

        void Register(YoloVirtualCameraDetector detector);
        void Unregister(YoloVirtualCameraDetector detector);
        void Publish(DetectionFrame frame);
    }
}