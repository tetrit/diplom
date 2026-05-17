using UnityEngine;
using Surveillance.Settings;

namespace Surveillance.Recognize
{

    public abstract class InferenceFactorySO : ScriptableObject
    {
        public abstract IInferenceEngine CreateEngine(RecognitionConfig config, IClassMapProvider classProvider);
    }
}