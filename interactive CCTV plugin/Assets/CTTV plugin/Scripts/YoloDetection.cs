using System;

[Serializable]
public struct YoloDetection
{
    public float x1;
    public float y1;
    public float x2;
    public float y2;
    public float confidence;
    public int classId;
    public string className;

    public YoloDetection(
        float x1,
        float y1,
        float x2,
        float y2,
        float confidence,
        int classId,
        string className)
    {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        this.confidence = confidence;
        this.classId = classId;
        this.className = className;
    }
}