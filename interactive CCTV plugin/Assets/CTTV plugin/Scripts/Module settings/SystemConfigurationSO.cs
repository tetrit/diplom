using System;
using System.Collections.Generic;
using UnityEngine;

namespace Surveillance.Settings
{[CreateAssetMenu(fileName = "SystemConfiguration", menuName = "Surveillance/Master System Configuration")]
    public class SystemConfigurationSO : ScriptableObject
    {[Header("Настройки камер")]
        public CameraConfig CameraSettings = new CameraConfig();[Header("Настройки распознавания")]
        public RecognitionConfig RecognitionSettings = new RecognitionConfig();[Header("Настройки отображения (Мониторы/UI)")]
        public DisplayConfig DisplaySettings = new DisplayConfig();
        
        [Header("Правила событий")]
        public List<Surveillance.Events.BaseRuleSO> EventRules = new List<Surveillance.Events.BaseRuleSO>();
    }

    [Serializable]
    public class CameraConfig
    {
        public int RenderWidth = 640;
        public int RenderHeight = 360;
        public int DepthBits = 24;
        public RenderTextureFormat Format = RenderTextureFormat.ARGB32;
        
        public int TargetFps = 10;
        public bool StartStreaming = true;
        [Range(10f, 120f)] public float FieldOfView = 60f;
        public float NearClipPlane = 0.1f;
        public float FarClipPlane = 1000f;
        public CameraClearFlags ClearFlags = CameraClearFlags.Skybox;
        public Color BackgroundColor = Color.black;
        public bool AllowHdr = false;
        public bool AllowMsaa = false;
    }

    [Serializable]
    public class RecognitionConfig
    {[Header("Движок инференса (Фабрика)")][Tooltip("Сюда перетаскиваем ScriptableObject нужной фабрики (например, DefaultYoloFactory)")]
        public Surveillance.Recognize.InferenceFactorySO EngineFactory;
        [Header("Универсальные параметры")]
        public int InputWidth = 416;
        public int InputHeight = 416;
        [Range(0.1f, 1f)] public float ConfidenceThreshold = 0.5f;
        public float DetectionInterval = 0.2f;
    }

    [Serializable]
    public class DisplayConfig
    {
        public bool ShowFallbackWhenSourceMissing = true;
        public bool AutoRebind = true;
        public Color BoundingBoxColor = Color.green;
        public int MaxBoxesOnScreen = 30;
    }
}