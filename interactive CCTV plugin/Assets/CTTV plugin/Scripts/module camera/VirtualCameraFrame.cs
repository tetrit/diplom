using UnityEngine;

namespace Surveillance.Cameras
{
    public readonly struct VirtualCameraFrame
    {
        public readonly string CameraId;
        public readonly long FrameIndex;
        public readonly float Timestamp;
        public readonly int Width;
        public readonly int Height;
        public readonly RenderTexture Texture;

        public VirtualCameraFrame(
            string cameraId,
            long frameIndex,
            float timestamp,
            RenderTexture texture)
        {
            CameraId = cameraId;
            FrameIndex = frameIndex;
            Timestamp = timestamp;
            Texture = texture;
            Width = texture != null ? texture.width : 0;
            Height = texture != null ? texture.height : 0;
        }
    }

    public readonly struct VirtualCameraCpuFrame
    {
        public readonly string CameraId;
        public readonly long FrameIndex;
        public readonly float Timestamp;
        public readonly Texture2D Texture;

        public VirtualCameraCpuFrame(
            string cameraId,
            long frameIndex,
            float timestamp,
            Texture2D texture)
        {
            CameraId = cameraId;
            FrameIndex = frameIndex;
            Timestamp = timestamp;
            Texture = texture;
        }
    }
    
    public readonly struct VirtualCameraParamForPredict
    {
        public readonly int width;
        public readonly int height;
        public readonly int targetCaptureFps;
        public readonly RenderTexture renderTexture;

        public VirtualCameraParamForPredict(
            int width,
            int height,
            int targetCaptureFps,
            RenderTexture renderTexture)
        {
            this.width = width;
            this.height = height;
            this.targetCaptureFps = targetCaptureFps;
            this.renderTexture = renderTexture;
        }
    }
}