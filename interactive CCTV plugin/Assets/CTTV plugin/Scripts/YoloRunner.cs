using UnityEngine;
using Unity.InferenceEngine;

public class YoloRunner : MonoBehaviour
{
    public ModelAsset modelAsset;
    public RenderTexture sourceTexture;
    public YoloClassMapProvider classMapProvider;
    public YoloOverlayCanvas overlayCanvas;
    public int inputWidth = 640;
    public int inputHeight = 640;
    public float confidenceThreshold = 0.25f;
    

    [Header("Detection interval")]
    public float detectionInterval = 0.3f; // раз в 0.3 сек

    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;

    private float nextDetectionTime = 0f;

    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, inputHeight, inputWidth));

        foreach (var input in runtimeModel.inputs)
            Debug.Log($"INPUT: {input.name} | shape = {input.shape}");

        foreach (var output in runtimeModel.outputs)
            Debug.Log($"OUTPUT: {output.name}");
    }

    void Update()
    {
        if (sourceTexture == null)
            return;

        if (Time.time < nextDetectionTime)
            return;

        nextDetectionTime = Time.time + detectionInterval;

        RunDetection();
    }

    void RunDetection()
    {
        TextureConverter.ToTensor(sourceTexture, inputTensor);
        worker.Schedule(inputTensor);

        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        float[] data = outputTensor.DownloadToArray();

        Debug.Log($"Output shape: {outputTensor.shape}");
        Debug.Log($"Output values count: {data.Length}");

        int detections = 300;
        int valuesPerDetection = 6;
        int found = 0;
        
        overlayCanvas?.ClearBoxes();


        for (int i = 0; i < detections; i++)
        {
            int offset = i * valuesPerDetection;

            float x1 = data[offset + 0];
            float y1 = data[offset + 1];
            float x2 = data[offset + 2];
            float y2 = data[offset + 3];
            float conf = data[offset + 4];
            float cls = data[offset + 5];

            if (conf < confidenceThreshold)
                continue;
            
            int classId = Mathf.RoundToInt(cls);
            string className = classMapProvider != null
                ? classMapProvider.GetClassName(classId)
                : $"unknown_{classId}";
            
            int drawIndex = found;

            overlayCanvas?.DrawBox(
                drawIndex,
                x1, y1, x2, y2,
                inputWidth, inputHeight,
                className, conf
            );

            found++;
            Debug.Log($"DET {found}: cls={className}, conf={conf:F2}, box=({x1:F1}, {y1:F1}) - ({x2:F1}, {y2:F1})");
        }

        Debug.Log($"Confident detections: {found}");
    }

    void OnDisable()
    {
        inputTensor?.Dispose();
        worker?.Dispose();
    }
}