using UnityEngine;
using Unity.InferenceEngine; // Замените на Unity.Sentis или Barracuda, если у вас другая версия пакета
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Surveillance.Recognize;

public class YoloInferenceEngine : System.IDisposable
{
    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;
    private RenderTexture _resizedTexture;
    
    private int inputWidth;
    private int inputHeight;
    private float confidenceThreshold;
    private YoloClassMapProvider classMapProvider;

    public YoloInferenceEngine(ModelAsset modelAsset, RecognizeProfileSO profile, YoloClassMapProvider classProvider)
    {
        inputWidth = profile.inputWidth;
        inputHeight = profile.inputHeight;
        confidenceThreshold = profile.confidenceThreshold;
        classMapProvider = classProvider;

        // Загружаем модель и создаем Worker
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, profile.BackendType);
        
        // Создаем тензор
        inputTensor = new Tensor<float>(new TensorShape(1, 3, inputHeight, inputWidth));
        
        // Создаем текстуру для ресайза
        _resizedTexture = new RenderTexture(inputWidth, inputHeight, 0, RenderTextureFormat.ARGB32);
        _resizedTexture.Create();
    }

    public async Task<List<BoundingBox>> RunInferenceAsync(RenderTexture sourceTexture)
    {
        if (sourceTexture == null) return new List<BoundingBox>();

        // 1. Копируем исходную текстуру камеры в текстуру нужного размера для нейросети
        Graphics.Blit(sourceTexture, _resizedTexture);
        
        // 2. Конвертируем в тензор
        TextureConverter.ToTensor(_resizedTexture, inputTensor);
        
        // 3. Выполняем расчет
        IEnumerator schedule = worker.ScheduleIterable(inputTensor);
        int layerCount = 0;
        while (schedule.MoveNext())
        {
            layerCount++;
            if (layerCount % 30 == 0) await Task.Yield(); // Даем Unity подышать
        }

        // 4. Читаем результат асинхронно
        var outputTensor = worker.PeekOutput() as Tensor<float>;
        var cpuCopy = await outputTensor.ReadbackAndCloneAsync();

        // 5. Обрабатываем данные
        var results = ProcessDetections(cpuCopy);
        
        cpuCopy.Dispose();
        return results;
    }

    private List<BoundingBox> ProcessDetections(Tensor<float> outputTensor)
    {
        List<BoundingBox> boxes = new List<BoundingBox>();
        int detections = 300; // Настройте под вашу модель YOLO
        int valuesPerDetection = 6;

        for (int i = 0; i < detections; i++)
        {
            int offset = i * valuesPerDetection;
            float conf = outputTensor[offset + 4]; 

            if (conf < confidenceThreshold)
                continue;

            float cls = outputTensor[offset + 5];
            int classId = Mathf.RoundToInt(cls);
            
            string className = classMapProvider != null && classMapProvider.IsLoaded
                ? classMapProvider.GetClassName(classId)
                : $"unknown_{classId}";

            boxes.Add(new BoundingBox
            {
                X1 = outputTensor[offset + 0],
                Y1 = outputTensor[offset + 1],
                X2 = outputTensor[offset + 2],
                Y2 = outputTensor[offset + 3],
                Confidence = conf,
                ClassId = classId,
                ClassName = className
            });
        }

        return boxes;
    }

    public void Dispose()
    {
        inputTensor?.Dispose();
        worker?.Dispose();
        if (_resizedTexture != null)
        {
            _resizedTexture.Release();
            Object.Destroy(_resizedTexture);
        }
    }
}