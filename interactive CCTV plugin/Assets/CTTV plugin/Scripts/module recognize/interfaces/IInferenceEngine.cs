using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Surveillance.Settings;

namespace Surveillance.Recognize
{
    // 1. Интерфейс для любого движка распознавания
    public interface IInferenceEngine : IDisposable
    {
        Task<List<BoundingBox>> RunInferenceAsync(RenderTexture sourceTexture);
        void UpdateConfig(RecognitionConfig config);
    }
}
