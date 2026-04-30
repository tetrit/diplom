using Unity.InferenceEngine;
using UnityEngine;

namespace Surveillance.Recognize
{
    [CreateAssetMenu(
        fileName = "RecognizeProfileSO",
        menuName = "Surveillance/Recognize Profile")]
    public class RecognizeProfileSO : ScriptableObject
    {
        [Header("Настройки")]
        [Space]
        [Header("Разрешение входного тензора")]
        [SerializeField]public int inputWidth = 416;
        [SerializeField]public int inputHeight = 416;
        [Space]
        [Header("Порог уверенности")]
        [SerializeField, Range(0.1f, 1f)]public float confidenceThreshold = 0.5f;
        [Space]
        [Header("Интвервал детекции")]
        [SerializeField, Min(0.1f)]public float detectionInterval = 0.2f;
        [Space]
        [Header("Бэкенд для запуска")]
        [SerializeField]public BackendType BackendType = BackendType.GPUCompute;



    }
}
