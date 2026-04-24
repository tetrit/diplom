using Surveillance.Settings;
using UnityEngine;

public class YoloClassMapProvider : MonoBehaviour
{
    private RecognitionConfig _currentConfig;

    public YoloClassMapData Data { get; private set; }

    public bool IsLoaded =>
        Data != null &&
        Data.class_names != null &&
        Data.class_names.Length > 0;

    private void Awake()
    {
        
        if (ConfigurationManager.Instance != null)
        {
            ConfigurationManager.Instance.OnConfigurationChanged += OnSettingsChanged;
            _currentConfig = ConfigurationManager.Instance.CurrentConfig.RecognitionSettings;
        }
        LoadAssignedJson();
    }
    

    private void OnSettingsChanged(SystemConfigurationSO config)
    {
        _currentConfig = config.RecognitionSettings;
        LoadAssignedJson();
    }

    public void LoadAssignedJson()
    {
        if (_currentConfig.TextAsset == null)
        {
            Debug.LogWarning("YoloClassMapProvider: JSON file is not assigned.");
            Data = null;
            return;
        }

        Data = JsonUtility.FromJson<YoloClassMapData>(_currentConfig.TextAsset.text);

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