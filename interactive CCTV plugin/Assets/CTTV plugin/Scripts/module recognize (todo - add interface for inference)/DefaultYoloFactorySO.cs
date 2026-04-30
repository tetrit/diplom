using Unity.InferenceEngine;
using UnityEngine;
using Surveillance.Settings;

namespace Surveillance.Recognize
{[CreateAssetMenu(fileName = "DefaultYoloFactory", menuName = "Surveillance/Inference/Default YOLO Factory")]
    public class DefaultYoloFactorySO : InferenceFactorySO
    {[Header("Специфичные настройки встроенного YOLO")]
        public ModelAsset Model;
        public BackendType BackendType = BackendType.GPUCompute;

        public override IInferenceEngine CreateEngine(RecognitionConfig config, IClassMapProvider classProvider)
        {
            if (Model == null)
            {
                Debug.LogError("YOLO Factory: Не назначена модель (ModelAsset)!");
                return null;
            }
            // ПЕРЕДАЕМ имена классов в движок через конструктор (см. ниже)
            return new YoloInferenceEngine(Model, BackendType, config, config.classes);
        }
    }
}