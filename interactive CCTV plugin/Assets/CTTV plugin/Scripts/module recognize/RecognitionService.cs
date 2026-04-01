using System;
using System.Collections.Generic;

namespace Surveillance.Recognition
{
    public sealed class RecognitionService : IRecognitionService
    {
        private readonly List<YoloVirtualCameraDetector> detectors = new List<YoloVirtualCameraDetector>();

        public IReadOnlyList<YoloVirtualCameraDetector> Detectors
        {
            get { return detectors; }
        }

        public event Action<YoloVirtualCameraDetector> DetectorRegistered;
        public event Action<YoloVirtualCameraDetector> DetectorUnregistered;
        public event Action<DetectionFrame> DetectionFrameProduced;

        public void Register(YoloVirtualCameraDetector detector)
        {
            if (detector == null || detectors.Contains(detector))
                return;

            detectors.Add(detector);
            DetectorRegistered?.Invoke(detector);
        }

        public void Unregister(YoloVirtualCameraDetector detector)
        {
            if (detector == null)
                return;

            if (detectors.Remove(detector))
                DetectorUnregistered?.Invoke(detector);
        }

        public void Publish(DetectionFrame frame)
        {
            if (frame == null)
                return;

            DetectionFrameProduced?.Invoke(frame);
        }
    }
}