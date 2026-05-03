using System;
using System.Collections.Generic;
using Surveillance.Recognize;

namespace Surveillance.Events
{
    public struct SystemEvent
    {
        public string EventId;           
        public DateTime Timestamp;       
        public int CameraId;             
        public string RuleName;          
        public string TargetClassName;   
        public int DetectedCount;        
        

        public List<BoundingBox> TriggeringBoxes; 
    }
}