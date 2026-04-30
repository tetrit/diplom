using UnityEngine;
using Unity.InferenceEngine;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Surveillance.Recognize;
using Surveillance.Settings;

public class YoloInferenceEngine : IInferenceEngine
{
    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;
    private RenderTexture _resizedTexture;
    
    private int inputWidth;
    private int inputHeight;
    private float confidenceThreshold;
    
    // Имена классов (теперь прямо тут!)
    private string[] _classNames;

    public YoloInferenceEngine(ModelAsset modelAsset, BackendType backendType, RecognitionConfig config, string[] classNames)
    {
        inputWidth = config.InputWidth;
        inputHeight = config.InputHeight;
        confidenceThreshold = config.ConfidenceThreshold;
        _classNames = classNames; // СОХРАНЯЕМ ИХ ТУТ!

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, backendType);
        
        inputTensor = new Tensor<float>(new TensorShape(1, 3, inputHeight, inputWidth));
        _resizedTexture = new RenderTexture(inputWidth, inputHeight, 0, RenderTextureFormat.ARGB32);
        _resizedTexture.Create();
    }

    public void UpdateConfig(RecognitionConfig config)
    {
        confidenceThreshold = config.ConfidenceThreshold;
    }

    public async Task<List<BoundingBox>> RunInferenceAsync(RenderTexture sourceTexture)
    {
        if (sourceTexture == null) return new List<BoundingBox>();

        Graphics.Blit(sourceTexture, _resizedTexture);
        TextureConverter.ToTensor(_resizedTexture, inputTensor);
        
        IEnumerator schedule = worker.ScheduleIterable(inputTensor);
        int layerCount = 0;
        while (schedule.MoveNext())
        {
            layerCount++;
            if (layerCount % 30 == 0) await Task.Yield();
        }

        var outputTensor = worker.PeekOutput() as Tensor<float>;
        var cpuCopy = await outputTensor.ReadbackAndCloneAsync();
        var results = ProcessDetections(cpuCopy);
        
        cpuCopy.Dispose();
        return results;
    }

    private List<BoundingBox> ProcessDetections(Tensor<float> outputTensor)
    {
        List<BoundingBox> boxes = new List<BoundingBox>();
        int detections = 300; 
        int valuesPerDetection = 6;

        for (int i = 0; i < detections; i++)
        {
            int offset = i * valuesPerDetection;
            float conf = outputTensor[offset + 4]; 

            if (conf < confidenceThreshold) continue;

            float cls = outputTensor[offset + 5];
            int classId = Mathf.RoundToInt(cls);
            string className = GetClassName(classId);

            boxes.Add(new BoundingBox
            {
                X1 = outputTensor[offset + 0], Y1 = outputTensor[offset + 1],
                X2 = outputTensor[offset + 2], Y2 = outputTensor[offset + 3],
                Confidence = conf, ClassId = classId, ClassName = className
            });
        }
        return boxes;
    }

    //TODO: переделать эту хуйню
    
    // Новый метод для получения имени класса
    private string GetClassName(int classId)
    {
        if (_classNames == null || _classNames.Length == 0) return $"unknown_{classId}";
        if (classId < 0 || classId >= _classNames.Length) return $"unknown_{classId}";
        return _classNames[classId];
    }

    public void Dispose()
    {
        inputTensor?.Dispose();
        worker?.Dispose();
        if (_resizedTexture != null) { _resizedTexture.Release(); Object.Destroy(_resizedTexture); }
    }
}