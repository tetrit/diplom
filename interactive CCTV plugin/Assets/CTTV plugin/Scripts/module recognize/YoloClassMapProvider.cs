

using Surveillance.Recognize;
using UnityEngine;


public class YoloClassMapProvider : IClassMapProvider
{

    public YoloClassMapData Data { get; private set; }

    public bool IsLoaded =>
        Data != null &&
        Data.class_names != null &&
        Data.class_names.Length > 0;
    

    public void LoadAssignedJson(string textAssetString)
    {
        if (textAssetString == null)
        {
            Data = null;
            return;
        }

        Data = JsonUtility.FromJson<YoloClassMapData>(textAssetString);

        if (Data == null || Data.class_names == null || Data.class_names.Length == 0)
        {
            Data = null;

        }
        
    }

    public string GetClassName(int classId)
    {
        if (!IsLoaded) return $"unknown_{classId}";
        if (classId < 0 || classId >= Data.class_names.Length) return $"unknown_{classId}";
        
        return Data.class_names[classId];
    }
}