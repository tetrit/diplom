using System.Collections.Generic;
using UnityEngine;
using Surveillance.Monitors;

public class MonitorManager : MonoBehaviour
{[Header("Настройки создания монитора")]
    [SerializeField] private MonitorSource monitorPrefab;
    [SerializeField] private VirtualMonitorProfileSO defaultProfile;
    [SerializeField] private Transform SpawnPoint;

    [Space]
    public int MonitorIDToRemove = -1;
    
    public MonitorSource MonitorPrefab => monitorPrefab;

    private Dictionary<int, MonitorSource> _monitorsDict = new Dictionary<int, MonitorSource>();

    private void Start()
    {
        RefreshMonitorDictionary();
    }
    
    public void AddMonitor()
    {
        if (_monitorsDict.Count == 0 && FindFirstObjectByType<MonitorSource>() != null)
        {
            RefreshMonitorDictionary();
        }
        
        int id = 0;
        while (_monitorsDict.ContainsKey(id))
        {
            id++;
        }
        
        MonitorSource monitor = Instantiate(MonitorPrefab, SpawnPoint.position, SpawnPoint.rotation);
        monitor.MonitorID = id;
        monitor.TargetCameraId = id;
        monitor.name = "Monitor_" + id;
        
        if (defaultProfile != null)
        {
            monitor.ApplyProfile(defaultProfile);
        }

        _monitorsDict.Add(id, monitor);
        Debug.Log("Monitor Spawned: " + id);
    }

    public void RemoveMonitor(int id)
    {
        if (_monitorsDict.Count == 0 && FindFirstObjectByType<MonitorSource>() != null)
        {
            RefreshMonitorDictionary();
        }
        
        if (_monitorsDict.ContainsKey(id))
        {
            MonitorSource spawned = _monitorsDict[id];
            if (Application.isPlaying && spawned != null)
            {
                Destroy(spawned.gameObject);
            }
            else if(!Application.isPlaying && spawned != null)
            { 
                DestroyImmediate(spawned.gameObject);
            }
            _monitorsDict.Remove(id);
        }
    }

    public void RefreshMonitorDictionary()
    {
        var monitors = FindObjectsByType<MonitorSource>(FindObjectsSortMode.InstanceID);
        System.Array.Sort(monitors, (a, b) => a.MonitorID.CompareTo(b.MonitorID));
        _monitorsDict.Clear();

        for (int i = 0; i < monitors.Length; i++)
        {
            if (!_monitorsDict.ContainsKey(monitors[i].MonitorID))
            {
                _monitorsDict.Add(monitors[i].MonitorID, monitors[i]);
            }
        }
    }
}