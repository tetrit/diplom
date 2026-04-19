using System.Collections.Generic;
using UnityEngine;

namespace Surveillance.Recognize
{
    public struct BoundingBox
    {
        public float X1, Y1, X2, Y2;
        public float Confidence;
        public int ClassId;
        public string ClassName;

        // Вспомогательное свойство для ширины и высоты (полезно для UI)
        public float Width => X2 - X1;
        public float Height => Y2 - Y1;
    }

    public struct DetectionResult
    {
        public int CameraId;         // ID камеры, с которой пришел кадр
        public int FrameWidth;       // Разрешение, под которое рассчитаны боксы
        public int FrameHeight;
        public List<BoundingBox> Boxes;
    }
}