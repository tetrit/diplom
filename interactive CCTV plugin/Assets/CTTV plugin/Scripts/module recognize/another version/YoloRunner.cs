using Surveillance.Cameras;
using UnityEngine;
using Unity.InferenceEngine;
using UnityEngine.Rendering;
using System.Threading.Tasks;
using System.Collections;

public class YoloRunner : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private VirtualCameraSource cameraSource;
    [SerializeField] private YoloClassMapProvider classMapProvider;
    [SerializeField] private VirtualCameraManager virtualCameraManager;
    [SerializeField] private int cameraId;

    public YoloOverlayCanvas overlayCanvas;

    public int inputWidth = 640;
    public int inputHeight = 640;

    [SerializeField, Range(0.1f, 1f)]
    private float confidenceThreshold = 0.25f;

    [SerializeField]
    private float detectionInterval = 0.2f;

    private RenderTexture _sourceTexture;
    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;

    private float nextDetectionTime = 0f;
    private bool inferenceInFlight = false;
    private bool disposed = false;
    private bool initialized = false;
    private Coroutine bindCoroutine;

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

        try
        {
            TextureConverter.ToTensor(_sourceTexture, inputTensor);
            worker.Schedule(inputTensor);

            var outputTensor = worker.PeekOutput() as Tensor<float>;
            var cpuCopy = await outputTensor.ReadbackAndCloneAsync();

            if (disposed || cpuCopy == null)
                return;

            ProcessDetections(cpuCopy);
            cpuCopy.Dispose();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            inferenceInFlight = false;
        }
    }

    void ProcessDetections(Tensor<float> outputTensor)
    {
        float[] data = outputTensor.DownloadToArray();

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