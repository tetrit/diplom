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
        
        public float Width => X2 - X1;
        public float Height => Y2 - Y1;
    }

    public struct DetectionResult
    {
        public int CameraId;         
        public int FrameWidth;       
        public int FrameHeight;
        public List<BoundingBox> Boxes;
    }
}