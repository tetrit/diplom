using System;
using System.Collections.Generic;
using UnityEngine;

namespace Surveillance.Recognition
{
    [Serializable]
    public struct NormalizedBoundingBox
    {
        // Координаты нормализованы в диапазон [0..1]
        // Начало координат — левый верхний угол изображения.
        public float x;
        public float y;
        public float width;
        public float height;

        public NormalizedBoundingBox(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    [Serializable]
    public struct DetectionResult
    {
        public int classIndex;
        public string className;
        public float confidence;
        public NormalizedBoundingBox box;

        public DetectionResult(int classIndex, string className, float confidence, NormalizedBoundingBox box)
        {
            this.classIndex = classIndex;
            this.className = className;
            this.confidence = confidence;
            this.box = box;
        }
    }

    public sealed class DetectionFrame
    {
        public string CameraId { get; private set; }
        public long SourceFrameIndex { get; private set; }
        public float Timestamp { get; private set; }
        public int SourceWidth { get; private set; }
        public int SourceHeight { get; private set; }
        public IReadOnlyList<DetectionResult> Detections { get; private set; }

        public DetectionFrame(
            string cameraId,
            long sourceFrameIndex,
            float timestamp,
            int sourceWidth,
            int sourceHeight,
            IReadOnlyList<DetectionResult> detections)
        {
            CameraId = cameraId;
            SourceFrameIndex = sourceFrameIndex;
            Timestamp = timestamp;
            SourceWidth = sourceWidth;
            SourceHeight = sourceHeight;
            Detections = detections;
        }
    }
}