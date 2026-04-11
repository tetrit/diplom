using Surveillance.Cameras;
using UnityEngine;
using Unity.InferenceEngine;
using UnityEngine.Rendering;

public class YoloRunner : MonoBehaviour
{
    [SerializeField]private ModelAsset modelAsset;
    [SerializeField]private VirtualCameraSource cameraSource;
    private RenderTexture _sourceTexture;
    [SerializeField]private YoloClassMapProvider classMapProvider;
    [SerializeField]private VirtualCameraManager virtualCameraManager;
    [SerializeField]private int cameraId = 0;
    
    //TODO: убрать это в другой скрипт
    public YoloOverlayCanvas overlayCanvas;
    public int inputWidth = 640;
    public int inputHeight = 640;
    [SerializeField][Range(0.1f, 1f)]private float confidenceThreshold = 0.25f;
    
    private float detectionInterval = 0.3f;

    private Model runtimeModel;
    private Worker worker;
    private Tensor<float> inputTensor;

    private float nextDetectionTime = 0f;
    
    
 
    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, inputHeight, inputWidth));

        if (virtualCameraManager != null)
            virtualCameraManager.cameraInitializedEvent += ConnectToCamera;

        if (cameraSource != null)
            ConnectToCamera(cameraSource);
        else if (virtualCameraManager != null)
        {
            var source = virtualCameraManager.GetVirtualCamera(cameraId);
            if (source != null)
                ConnectToCamera(source);
        }
    }

    void Update()
    {
        
        //TODO: короутина, ию ноу?
        if (_sourceTexture == null)
        {
            //Debug.Log($"No source texture assigned");
            return;
        }


        if (Time.time < nextDetectionTime)
            return;

        nextDetectionTime = Time.time + detectionInterval;

        RunDetection();
    }

    void RunDetection()
    {
        
        TextureConverter.ToTensor(_sourceTexture, inputTensor);
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
    }

    void OnDisable()
    {
        inputTensor?.Dispose();
        worker?.Dispose();
    }
    
    void setup(RenderTexture  texture, int fps)
    {
        _sourceTexture = texture;
        detectionInterval = 1f / fps;
    }

    void ConnectToCamera(VirtualCameraSource source)
    {
        if (source == null || source.CameraId != cameraId)
        {
            return;
        }

        Debug.Log("[YOLO] Camera event received");
        cameraSource = source;
        StartCoroutine(BindWhenReady(source));
    }

    private System.Collections.IEnumerator BindWhenReady(VirtualCameraSource source)
    {
        while (source != null && source.OutputTexture == null)
            yield return null;

        if (source == null)
            yield break;

        _sourceTexture = source.OutputTexture;
        detectionInterval = 1f / Mathf.Max(1, source.fps);

        Debug.Log($"[YOLO] Bound to texture: {_sourceTexture.name}, fps={source.fps}");
    }
    
}