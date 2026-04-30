using System;

[Serializable]
public class YoloClassMapData
{
    public string model_id;
    public string source_model_file;
    public int version;
    public string[] class_names;
    public bool IsLoaded { get; }
}