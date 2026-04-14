using Surveillance.Cameras;
using UnityEngine;
using Unity.InferenceEngine;
using UnityEngine.Rendering;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

public class YoloRunner : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private VirtualCameraSource cameraSource;
    [SerializeField] private YoloClassMapProvider classMapProvider;
    [SerializeField] private VirtualCameraManager virtualCameraManager;
    [SerializeField] private int cameraId;

    public YoloOverlayCanvas overlayCanvas;

    public int inputWidth = 416;
    public int inputHeight = 416;

    [SerializeField, Range(0.1f, 1f)]
    private float confidenceThreshold = 0.25f;

    [SerializeField]
    private float detectionInterval = 0.2f;

    private RenderTexture _sourceTexture;
    private RenderTexture _resizedTexture;
    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;

    private float nextDetectionTime = 0f;
    private bool inferenceInFlight = false;
    private bool disposed = false;
    private bool initialized = false;
    private Coroutine bindCoroutine;
    
    private static readonly SemaphoreSlim _globalInferenceLock = new SemaphoreSlim(1, 1);

    public int CameraId => cameraId;

    public float ConfidenceThreshold
    {
        get => confidenceThreshold;
        set => confidenceThreshold = value;
    }

    void Awake()
    {
        virtualCameraManager = FindObjectOfType<VirtualCameraManager>();
        InitModelIfNeeded();
        
        _resizedTexture = new RenderTexture(inputWidth, inputHeight, 0, RenderTextureFormat.ARGB32);
        _resizedTexture.Create();
    }

    void Update()
    {
        if (!initialized)
            return;

        if (_sourceTexture == null || inferenceInFlight)
            return;

        if (Time.time < nextDetectionTime)
            return;

        nextDetectionTime = Time.time + detectionInterval;
        _ = RunDetectionAsync();
    }

    private void InitModelIfNeeded()
    {
        if (runtimeModel != null && worker != null && inputTensor != null)
            return;

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, inputHeight, inputWidth));
    }

    public void InitializeFromMonitor(int newCameraId, float newConfidenceThreshold)
    {
        cameraId = newCameraId;
        confidenceThreshold = newConfidenceThreshold;
        initialized = true;
    }

    public void BindToCamera(VirtualCameraSource source)
    {
        if (source == null)
        {
            cameraSource = null;
            _sourceTexture = null;
            return;
        }

        if (source.CameraId != cameraId)
            return;

        cameraSource = source;

        if (bindCoroutine != null)
            StopCoroutine(bindCoroutine);

        bindCoroutine = StartCoroutine(BindWhenReady(cameraSource));
    }

    public void BindToCameraById()
    {
        if (virtualCameraManager == null)
            return;

        var source = virtualCameraManager.GetVirtualCamera(cameraId);
        BindToCamera(source);
    }

    async Task RunDetectionAsync()
    {
        inferenceInFlight = true;
        
        await _globalInferenceLock.WaitAsync();


        try
        {
            if (disposed) return;

            // Быстрый ресайз силами GPU
            Graphics.Blit(_sourceTexture, _resizedTexture);
            
            TextureConverter.ToTensor(_resizedTexture, inputTensor);
            
            // Распределяем нагрузку на кадры
            IEnumerator schedule = worker.ScheduleIterable(inputTensor);
            int layerCount = 0;
            while (schedule.MoveNext())
            {
                layerCount++;
                if (layerCount % 30 == 0) await Task.Yield(); // Даем Unity подышать
            }

            var outputTensor = worker.PeekOutput() as Tensor<float>;
            var cpuCopy = await outputTensor.ReadbackAndCloneAsync();

            if (disposed || cpuCopy == null) return;

            ProcessDetections(cpuCopy);
            cpuCopy.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            // Освобождаем очередь для следующей камеры
            _globalInferenceLock.Release();
            inferenceInFlight = false;
        }
    }

    void ProcessDetections(Tensor<float> outputTensor)
    {
        // ВНИМАНИЕ: Больше никакого DownloadToArray()! 
        // Читаем напрямую из памяти (cpuCopy), что спасает сборщик мусора от перегрузок.

        int detections = 300;
        int valuesPerDetection = 6;
        int found = 0;

        overlayCanvas?.ClearBoxes();

        for (int i = 0; i < detections; i++)
        {
            int offset = i * valuesPerDetection;

            // Обращаемся напрямую к тензору по индексу
            float conf = outputTensor[offset + 4]; 

            if (conf < confidenceThreshold)
                continue;

            float x1 = outputTensor[offset + 0];
            float y1 = outputTensor[offset + 1];
            float x2 = outputTensor[offset + 2];
            float y2 = outputTensor[offset + 3];
            float cls = outputTensor[offset + 5];

            int classId = Mathf.RoundToInt(cls);
            string className = classMapProvider != null
                ? classMapProvider.GetClassName(classId)
                : $"unknown_{classId}";

            overlayCanvas?.DrawBox(
                found,
                x1, y1, x2, y2,
                inputWidth, inputHeight,
                className, conf
            );

            found++;
        }
    }

    private IEnumerator BindWhenReady(VirtualCameraSource source)
    {
        while (source != null && source.OutputTexture == null)
            yield return null;

        if (source == null)
            yield break;

        _sourceTexture = source.OutputTexture;
    }

    void OnDisable()
    {
        disposed = true;
        inputTensor?.Dispose();
        worker?.Dispose();
    }
}