using UnityEngine;

public class YoloClassMapProvider : MonoBehaviour
{
    [SerializeField] private TextAsset classMapJson;

    public YoloClassMapData Data { get; private set; }

    public bool IsLoaded =>
        Data != null &&
        Data.class_names != null &&
        Data.class_names.Length > 0;

    private void Awake()
    {
        LoadAssignedJson();
    }

    public void LoadAssignedJson()
    {
        if (classMapJson == null)
        {
            Debug.LogWarning("YoloClassMapProvider: JSON file is not assigned.");
            Data = null;
            return;
        }

        Data = JsonUtility.FromJson<YoloClassMapData>(classMapJson.text);

        if (Data == null || Data.class_names == null || Data.class_names.Length == 0)
        {
            Debug.LogWarning("YoloClassMapProvider: JSON loaded, but class_names is empty.");
            Data = null;
            return;
        }

        Debug.Log($"Class map loaded: {Data.model_id}, classes = {Data.class_names.Length}");
    }

    public string GetClassName(int classId)
    {
        if (!IsLoaded)
            return $"unknown_{classId}";

        if (classId < 0 || classId >= Data.class_names.Length)
            return $"unknown_{classId}";

        return Data.class_names[classId];
    }
}