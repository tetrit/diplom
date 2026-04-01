using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Surveillance.Cameras;
using Unity.InferenceEngine;

namespace Surveillance.Recognition
{
    public sealed class YoloVirtualCameraDetector : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private YoloDetectorProfileSO profile;

        [Header("Source")]
        [SerializeField] private string cameraId = "camera_01";

        private IVirtualCameraService cameraService;
        private IRecognitionService recognitionService;
        private VirtualCameraSource boundCamera;

        private Model runtimeModel;
        private Worker worker;
        private Tensor<float> inputTensor;
        private TextureTransform textureTransform;
        private string[] classLabels;

        private bool inferenceRunning;
        private bool isInitialized;
        private float nextInferenceTime;

        public string CameraId
        {
            get { return cameraId; }
        }

        public event Action<DetectionFrame> DetectionFrameProduced;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(cameraId))
                cameraId = gameObject.name;
        }

        private void Start()
        {
            if (profile == null || profile.modelAsset == null)
            {
                Debug.LogError("[" + name + "] YoloDetectorProfileSO or modelAsset is not assigned.");
                enabled = false;
                return;
            }

            InitializeRuntime();
            BindServicesAndCamera();

            if (profile.warmupOnStart)
                _ = WarmupAsync();
        }

        private void Update()
        {
            if (!isInitialized || inferenceRunning || boundCamera == null)
                return;

            if (Time.unscaledTime < nextInferenceTime)
                return;

            _ = RunInferenceAsync();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCameraService();

            if (recognitionService != null)
                recognitionService.Unregister(this);

            if (inputTensor != null)
            {
                inputTensor.Dispose();
                inputTensor = null;
            }

            if (worker != null)
            {
                worker.Dispose();
                worker = null;
            }
        }

        public void SetCameraId(string newCameraId)
        {
            cameraId = newCameraId;
            RebindCamera();
        }

        private void InitializeRuntime()
        {
            runtimeModel = ModelLoader.Load(profile.modelAsset);
            worker = new Worker(runtimeModel, profile.backendType);

            inputTensor = new Tensor<float>(
                new TensorShape(1, profile.inputChannels, profile.inputHeight, profile.inputWidth));

            textureTransform = new TextureTransform()
                .SetDimensions(profile.inputWidth, profile.inputHeight, profile.inputChannels);

            classLabels = ParseLabels(profile.labelsAsset);

            isInitialized = true;
            nextInferenceTime = Time.unscaledTime;
        }

        private void BindServicesAndCamera()
        {
            if (!ServiceLocator.TryGet<IVirtualCameraService>(out cameraService))
            {
                Debug.LogWarning("[" + name + "] IVirtualCameraService is not registered.");
            }
            else
            {
                cameraService.CameraRegistered += OnCameraRegistered;
                cameraService.CameraUnregistered += OnCameraUnregistered;
                RebindCamera();
            }

            if (ServiceLocator.TryGet<IRecognitionService>(out recognitionService))
            {
                recognitionService.Register(this);
            }
        }

        private void RebindCamera()
        {
            boundCamera = null;

            if (cameraService == null)
                return;

            VirtualCameraSource source;
            if (cameraService.TryGetCamera(cameraId, out source))
                boundCamera = source;
        }

        private async Task WarmupAsync()
        {
            if (!isInitialized || worker == null)
                return;

            try
            {
                worker.Schedule(inputTensor);
                Tensor<float> output = worker.PeekOutput() as Tensor<float>;
                if (output != null)
                {
                    Tensor<float> cpuCopy = await output.ReadbackAndCloneAsync();
                    cpuCopy.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + name + "] Warmup failed: " + e.Message);
            }
        }

        private async Task RunInferenceAsync()
        {
            if (inferenceRunning || boundCamera == null || boundCamera.OutputTexture == null)
                return;

            inferenceRunning = true;

            try
            {
                TextureConverter.ToTensor(boundCamera.OutputTexture, inputTensor, textureTransform);

                if (profile.scheduleOverMultipleFrames)
                {
                    IEnumerator schedule = worker.ScheduleIterable(inputTensor);
                    int layersBudget = Mathf.Max(1, profile.layersPerFrame);
                    int iteration = 0;

                    while (schedule.MoveNext())
                    {
                        iteration++;
                        if (iteration % layersBudget == 0)
                            await Task.Yield();
                    }
                }
                else
                {
                    worker.Schedule(inputTensor);
                }

                Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
                if (outputTensor == null)
                    return;

                Tensor<float> cpuOutput = await outputTensor.ReadbackAndCloneAsync();

                List<DetectionResult> detections =
                    YoloCpuDecoder.Decode(cpuOutput, profile, classLabels);

                cpuOutput.Dispose();

                DetectionFrame frame = new DetectionFrame(
                    boundCamera.CameraId,
                    0,
                    Time.unscaledTime,
                    boundCamera.OutputTexture.width,
                    boundCamera.OutputTexture.height,
                    detections);

                DetectionFrameProduced?.Invoke(frame);

                if (recognitionService != null)
                    recognitionService.Publish(frame);

                nextInferenceTime = Time.unscaledTime + (1f / Mathf.Max(1, profile.targetInferenceFps));
            }
            catch (Exception e)
            {
                Debug.LogError("[" + name + "] Inference failed: " + e);
            }
            finally
            {
                inferenceRunning = false;
            }
        }

        private void OnCameraRegistered(VirtualCameraSource source)
        {
            if (source == null)
                return;

            if (source.CameraId == cameraId)
                boundCamera = source;
        }

        private void OnCameraUnregistered(VirtualCameraSource source)
        {
            if (source == null || boundCamera == null)
                return;

            if (source == boundCamera)
                boundCamera = null;
        }

        private void UnsubscribeFromCameraService()
        {
            if (cameraService == null)
                return;

            cameraService.CameraRegistered -= OnCameraRegistered;
            cameraService.CameraUnregistered -= OnCameraUnregistered;
            cameraService = null;
        }

        private static string[] ParseLabels(TextAsset labelsAsset)
        {
            if (labelsAsset == null || string.IsNullOrWhiteSpace(labelsAsset.text))
                return new string[0];

            string[] lines = labelsAsset.text.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].Trim();

            return lines;
        }
    }
}