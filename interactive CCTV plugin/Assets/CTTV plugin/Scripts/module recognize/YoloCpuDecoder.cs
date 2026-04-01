using System.Collections.Generic;
using Unity.InferenceEngine;
using UnityEngine;

namespace Surveillance.Recognition
{
    public static class YoloCpuDecoder
    {
        private struct Candidate
        {
            public int classIndex;
            public float confidence;
            public NormalizedBoundingBox box;
        }

        public static List<DetectionResult> Decode(
            Tensor<float> outputTensor,
            YoloDetectorProfileSO profile,
            string[] classLabels)
        {
            List<DetectionResult> results = new List<DetectionResult>();

            if (outputTensor == null || profile == null)
                return results;

            TensorShape shape = outputTensor.shape;
            if (shape.rank < 2 || shape.rank > 3)
            {
                Debug.LogWarning("YOLO decoder expects rank 2 or rank 3 tensor.");
                return results;
            }

            float[] data = outputTensor.DownloadToArray();

            int d0;
            int d1;
            int d2;

            if (shape.rank == 3)
            {
                d0 = shape[0];
                d1 = shape[1];
                d2 = shape[2];
            }
            else
            {
                d0 = 1;
                d1 = shape[0];
                d2 = shape[1];
            }

            if (d0 != 1)
            {
                Debug.LogWarning("Only batch size 1 is supported in current decoder.");
            }

            int labelsCount = classLabels != null ? classLabels.Length : 0;

            bool channelsFirst = ResolveChannelsFirst(profile.outputLayout, d1, d2, labelsCount);

            int featureCount = channelsFirst ? d1 : d2;
            int candidateCount = channelsFirst ? d2 : d1;

            bool hasObjectness = ResolveObjectnessMode(profile.confidenceMode, featureCount, labelsCount);
            int classStart = hasObjectness ? 5 : 4;

            if (featureCount <= classStart)
            {
                Debug.LogWarning("YOLO decoder: feature count is too small.");
                return results;
            }

            List<Candidate> candidates = new List<Candidate>();

            for (int candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++)
            {
                float cx = ReadValue(data, channelsFirst, candidateIndex, 0, featureCount, candidateCount);
                float cy = ReadValue(data, channelsFirst, candidateIndex, 1, featureCount, candidateCount);
                float w = ReadValue(data, channelsFirst, candidateIndex, 2, featureCount, candidateCount);
                float h = ReadValue(data, channelsFirst, candidateIndex, 3, featureCount, candidateCount);

                float objectness = hasObjectness
                    ? ReadValue(data, channelsFirst, candidateIndex, 4, featureCount, candidateCount)
                    : 1f;

                int bestClassIndex = -1;
                float bestClassScore = 0f;

                for (int classIndex = classStart; classIndex < featureCount; classIndex++)
                {
                    float score = ReadValue(data, channelsFirst, candidateIndex, classIndex, featureCount, candidateCount);
                    if (score > bestClassScore)
                    {
                        bestClassScore = score;
                        bestClassIndex = classIndex - classStart;
                    }
                }

                if (bestClassIndex < 0)
                    continue;

                float confidence = hasObjectness ? objectness * bestClassScore : bestClassScore;
                if (confidence < profile.confidenceThreshold)
                    continue;

                NormalizedBoundingBox normalizedBox = ConvertToNormalizedBox(
                    cx, cy, w, h, profile.inputWidth, profile.inputHeight);

                if (normalizedBox.width <= 0f || normalizedBox.height <= 0f)
                    continue;

                Candidate candidate = new Candidate();
                candidate.classIndex = bestClassIndex;
                candidate.confidence = confidence;
                candidate.box = normalizedBox;

                candidates.Add(candidate);
            }

            candidates.Sort(delegate (Candidate a, Candidate b)
            {
                return b.confidence.CompareTo(a.confidence);
            });

            List<Candidate> nms = ApplyNms(candidates, profile.nmsIouThreshold);

            for (int i = 0; i < nms.Count; i++)
            {
                Candidate item = nms[i];

                string className;
                if (classLabels != null && item.classIndex >= 0 && item.classIndex < classLabels.Length)
                    className = classLabels[item.classIndex];
                else
                    className = "class_" + item.classIndex;

                results.Add(new DetectionResult(
                    item.classIndex,
                    className,
                    item.confidence,
                    item.box));
            }

            return results;
        }

        private static bool ResolveChannelsFirst(
            YoloOutputLayout layout,
            int d1,
            int d2,
            int labelsCount)
        {
            if (layout == YoloOutputLayout.ChannelsFirst)
                return true;

            if (layout == YoloOutputLayout.ChannelsLast)
                return false;

            int minFeaturesWithoutObj = labelsCount + 4;
            int minFeaturesWithObj = labelsCount + 5;

            bool d1LooksLikeFeatures = d1 == minFeaturesWithoutObj || d1 == minFeaturesWithObj;
            bool d2LooksLikeFeatures = d2 == minFeaturesWithoutObj || d2 == minFeaturesWithObj;

            if (d1LooksLikeFeatures && !d2LooksLikeFeatures)
                return true;

            if (!d1LooksLikeFeatures && d2LooksLikeFeatures)
                return false;

            return d1 < d2;
        }

        private static bool ResolveObjectnessMode(
            YoloConfidenceMode mode,
            int featureCount,
            int labelsCount)
        {
            if (mode == YoloConfidenceMode.ObjectnessTimesClass)
                return true;

            if (mode == YoloConfidenceMode.ClassOnly)
                return false;

            if (labelsCount > 0)
                return featureCount == labelsCount + 5;

            return featureCount > 84;
        }

        private static float ReadValue(
            float[] data,
            bool channelsFirst,
            int candidateIndex,
            int featureIndex,
            int featureCount,
            int candidateCount)
        {
            if (channelsFirst)
            {
                int flatIndex = featureIndex * candidateCount + candidateIndex;
                return data[flatIndex];
            }
            else
            {
                int flatIndex = candidateIndex * featureCount + featureIndex;
                return data[flatIndex];
            }
        }

        private static NormalizedBoundingBox ConvertToNormalizedBox(
            float cx,
            float cy,
            float w,
            float h,
            int inputWidth,
            int inputHeight)
        {
            bool alreadyNormalized =
                cx <= 1.5f && cy <= 1.5f && w <= 1.5f && h <= 1.5f;

            float x;
            float y;
            float width;
            float height;

            if (alreadyNormalized)
            {
                x = cx - (w * 0.5f);
                y = cy - (h * 0.5f);
                width = w;
                height = h;
            }
            else
            {
                x = (cx - (w * 0.5f)) / inputWidth;
                y = (cy - (h * 0.5f)) / inputHeight;
                width = w / inputWidth;
                height = h / inputHeight;
            }

            float xMin = Mathf.Clamp01(x);
            float yMin = Mathf.Clamp01(y);
            float xMax = Mathf.Clamp01(x + width);
            float yMax = Mathf.Clamp01(y + height);

            return new NormalizedBoundingBox(
                xMin,
                yMin,
                Mathf.Max(0f, xMax - xMin),
                Mathf.Max(0f, yMax - yMin));
        }

        private static List<Candidate> ApplyNms(List<Candidate> source, float iouThreshold)
        {
            List<Candidate> kept = new List<Candidate>();

            for (int i = 0; i < source.Count; i++)
            {
                Candidate candidate = source[i];
                bool shouldKeep = true;

                for (int j = 0; j < kept.Count; j++)
                {
                    if (kept[j].classIndex != candidate.classIndex)
                        continue;

                    float iou = IoU(kept[j].box, candidate.box);
                    if (iou > iouThreshold)
                    {
                        shouldKeep = false;
                        break;
                    }
                }

                if (shouldKeep)
                    kept.Add(candidate);
            }

            return kept;
        }

        private static float IoU(NormalizedBoundingBox a, NormalizedBoundingBox b)
        {
            float ax1 = a.x;
            float ay1 = a.y;
            float ax2 = a.x + a.width;
            float ay2 = a.y + a.height;

            float bx1 = b.x;
            float by1 = b.y;
            float bx2 = b.x + b.width;
            float by2 = b.y + b.height;

            float interX1 = Mathf.Max(ax1, bx1);
            float interY1 = Mathf.Max(ay1, by1);
            float interX2 = Mathf.Min(ax2, bx2);
            float interY2 = Mathf.Min(ay2, by2);

            float interW = Mathf.Max(0f, interX2 - interX1);
            float interH = Mathf.Max(0f, interY2 - interY1);
            float interArea = interW * interH;

            float areaA = a.width * a.height;
            float areaB = b.width * b.height;
            float union = areaA + areaB - interArea;

            if (union <= 0f)
                return 0f;

            return interArea / union;
        }
    }
}