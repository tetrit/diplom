using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.InferenceEngine;

[DisallowMultipleComponent]
public class YoloRunner : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private CctvCameraSource source;
    [SerializeField] private YoloClassMapProvider classMapProvider;

    [Header("Model")]
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private int inputWidth = 640;
    [SerializeField] private int inputHeight = 640;
    [SerializeField] private float confidenceThreshold = 0.25f;
    [SerializeField] private int maxDetections = 300;

    [Header("Timing")]
    [SerializeField] private float detectionInterval = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = false;

    private readonly List<YoloDetection> currentDetections = new();

    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;
    private float nextDetectionTime;
    private bool initialized;

    public CctvCameraSource Source => source;
    public int InputWidth => inputWidth;
    public int InputHeight => inputHeight;
    public IReadOnlyList<YoloDetection> CurrentDetections => currentDetections;

    public event Action<YoloRunner> DetectionsUpdated;

    private void Reset()
    {
        if (source == null)
            source = GetComponent<CctvCameraSource>();
    }

    private void Awake()
    {
        if (source == null)
            source = GetComponent<CctvCameraSource>();
    }

    private void OnEnable()
    {
        TryInitialize();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (source == null || source.OutputTexture == null)
            return;

        if (Time.time < nextDetectionTime)
            return;

        nextDetectionTime = Time.time + detectionInterval;
        RunDetection();
    }

    private void OnDisable()
    {
        currentDetections.Clear();
        DetectionsUpdated?.Invoke(this);
        DisposeRuntime();
    }

    private void OnDestroy()
    {
        DisposeRuntime();
    }

    private void TryInitialize()
    {
        if (initialized)
            return;

        if (source == null)
            source = GetComponent<CctvCameraSource>();

        if (modelAsset == null)
        {
            Debug.LogError($"{name}: YoloRunner - не назначен Model Asset.");
            return;
        }

        if (inputWidth <= 0) inputWidth = 640;
        if (inputHeight <= 0) inputHeight = 640;
        if (maxDetections <= 0) maxDetections = 300;
        if (detectionInterval <= 0f) detectionInterval = 0.3f;

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, inputHeight, inputWidth));

        if (verboseLogging)
        {
            foreach (var input in runtimeModel.inputs)
                Debug.Log($"{name} INPUT: {input.name} | shape = {input.shape}");

            foreach (var output in runtimeModel.outputs)
                Debug.Log($"{name} OUTPUT: {output.name}");
        }

        initialized = true;
    }

    private void RunDetection()
    {
        RenderTexture texture = source != null ? source.OutputTexture : null;
        if (texture == null)
            return;

        currentDetections.Clear();

        TextureConverter.ToTensor(texture, inputTensor);
        worker.Schedule(inputTensor);

        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        if (outputTensor == null)
        {
            DetectionsUpdated?.Invoke(this);
            return;
        }

        float[] data = outputTensor.DownloadToArray();
        if (data == null || data.Length == 0)
        {
            DetectionsUpdated?.Invoke(this);
            return;
        }

        const int valuesPerDetection = 6;
        int detectionsCount = Mathf.Min(maxDetections, data.Length / valuesPerDetection);

        for (int i = 0; i < detectionsCount; i++)
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

            currentDetections.Add(new YoloDetection(
                x1,
                y1,
                x2,
                y2,
                conf,
                classId,
                className
            ));
        }

        if (verboseLogging)
            Debug.Log($"{name}: detections = {currentDetections.Count}");

        DetectionsUpdated?.Invoke(this);
    }

    private void DisposeRuntime()
    {
        initialized = false;

        inputTensor?.Dispose();
        worker?.Dispose();

        inputTensor = null;
        worker = null;
        runtimeModel = null;
    }
}