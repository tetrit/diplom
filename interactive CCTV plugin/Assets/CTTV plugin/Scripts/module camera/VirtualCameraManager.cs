using System;
using System.Collections.Generic;
using Surveillance.Cameras;
using Surveillance.Settings;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour
{
    [SerializeField] private VirtualCameraSource cameraPrefab;
    [SerializeField] private int cameraIDToRemove = 0;
    [SerializeField] private Transform SpawnPoint;
    
    public VirtualCameraSource CameraPrefab => cameraPrefab;
    private Dictionary<int, VirtualCameraSource> _virtualcameraDict = new Dictionary<int, VirtualCameraSource>();

    public int CameraIDToRemove => cameraIDToRemove;
    
    public event Action<VirtualCameraSource> cameraInitializedEvent;
    public event Action<int> cameraRemovedEvent;

    void Start()
    {
        // Подписка на централизованные настройки
        if (ConfigurationManager.Instance != null)
        {
            ConfigurationManager.Instance.OnConfigurationChanged += OnSettingsChanged;
        }

        if (_virtualcameraDict.Count == 0 && FindAnyObjectByType<VirtualCameraSource>() != null)
        {
            FillVirtualCameraDict();
        }
    }

    private void OnDestroy()
    {
        if (ConfigurationManager.Instance != null)
        {
            ConfigurationManager.Instance.OnConfigurationChanged -= OnSettingsChanged;
        }
    }
    
    private void OnSettingsChanged(SystemConfigurationSO config)
    {
        foreach (var cam in _virtualcameraDict.Values)
        {
            if (cam != null) cam.ApplyConfig(config.CameraSettings);
        }
    }

    private void AddCamera(VirtualCameraSource virtualcamera)
    {
        int id = 0;
        if (_virtualcameraDict.Count == 0 && FindAnyObjectByType<VirtualCameraSource>() != null) FillVirtualCameraDict();
        while (_virtualcameraDict.ContainsKey(id)) id++;
        
        VirtualCameraSource spawned = Instantiate(virtualcamera, SpawnPoint.position, SpawnPoint.rotation);
        spawned.CameraId = id;
        spawned.name = "CCTV cam_" + spawned.CameraId;
        
        // Применяем настройки сразу при создании
        if (ConfigurationManager.Instance != null)
            spawned.ApplyConfig(ConfigurationManager.Instance.CurrentConfig.CameraSettings);
        
        spawned.Initialize();
        _virtualcameraDict.Add(spawned.CameraId, spawned);
        cameraInitializedEvent?.Invoke(spawned);
    }

    private void removeCamera(int cameraID)
    {
        if (_virtualcameraDict.Count == 0 && FindAnyObjectByType<VirtualCameraSource>() != null) FillVirtualCameraDict();
        
        VirtualCameraSource spawned = _virtualcameraDict.ContainsKey(cameraID) ? _virtualcameraDict[cameraID] : null;
        if (spawned != null)
        {
            if (Application.isPlaying) Destroy(spawned.gameObject);
            else DestroyImmediate(spawned.gameObject);
        }

        _virtualcameraDict.Remove(cameraID);
        cameraRemovedEvent?.Invoke(cameraID);
    }

    public void SpawnCameraEditor(VirtualCameraSource virtualcamera) => AddCamera(virtualcamera);
    public void DestroyCameraEditor(int cameraID) => removeCamera(cameraID);

    public VirtualCameraSource GetVirtualCamera(int cameraId)
    {
        if (_virtualcameraDict.Count == 0 && FindAnyObjectByType<VirtualCameraSource>() != null) FillVirtualCameraDict();
        return _virtualcameraDict.ContainsKey(cameraId) ? _virtualcameraDict[cameraId] : null;
    }

    private void FillVirtualCameraDict()
    {
        var cameras = FindObjectsByType<VirtualCameraSource>(FindObjectsSortMode.None);
        foreach (var cam in cameras) _virtualcameraDict.TryAdd(cam.CameraId, cam);
    }
}