using System;
using System.Collections.Generic;
using UnityEngine;

namespace Surveillance.Settings
{
    // Главный класс конфигурации, объединяющий все настройки системы
    [Serializable]
    public class SystemConfiguration
    {
        public CameraConfig CameraSettings = new CameraConfig();
        public RecognitionConfig RecognitionSettings = new RecognitionConfig();
        public DisplayConfig DisplaySettings = new DisplayConfig();
        public List<RuleConfig> EventRules = new List<RuleConfig>();
    }

    [Serializable]
    public class CameraConfig
    {[Header("Настройки захвата")]
        public int TargetFps = 10;
        public int RenderWidth = 640;
        public int RenderHeight = 360;

        [Header("Параметры объектива")]
        public float FieldOfView = 60f;
        public float NearClipPlane = 0.1f;
        public float FarClipPlane = 1000f;
    }

    [Serializable]
    public class RecognitionConfig
    {
        public int InputWidth = 416;
        public int InputHeight = 416;
        public float ConfidenceThreshold = 0.5f;
        public float DetectionInterval = 0.2f;
    }

    [Serializable]
    public class DisplayConfig
    {
        public bool ShowFallbackTexture = true;
        public bool AutoRebindCameras = true;
        public Color BoundingBoxColor = Color.green;
        public int MaxBoxesOnScreen = 30;
    }

    [Serializable]
    public class RuleConfig
    {
        public string RuleName;
        public bool IsActive;
        public string TargetClassName;
        public int MinimumObjectsCount;
        public float MinimumConfidence;
        public float CooldownSeconds;
    }
}