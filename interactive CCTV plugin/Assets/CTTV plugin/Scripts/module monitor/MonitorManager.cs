using System;
using System.Collections.Generic;
using Surveillance.Cameras;
using UnityEngine;

public class MonitorManager : MonoBehaviour
{
    [SerializeField] private MonitorSource monitorPrefab;
    public int MonitorIDToRemove = -1;
    public MonitorSource MonitorPrefab
    {
        get{return monitorPrefab;}
    }

    private void Start()
    {
        RefreshMonitorDictionary();
    }

    private Dictionary<int, MonitorSource> _monitorsDict = new Dictionary<int, MonitorSource>();
    
    public void AddMonitor()
    {
        if (_monitorsDict.Count == 0 && FindObjectOfType<MonitorSource>() != null)
        {
            RefreshMonitorDictionary();
        }
        
        int id = 0;
        while (_monitorsDict.ContainsKey(id))
        {
            id++;
        }
        
        MonitorSource monitor = Instantiate(MonitorPrefab);
        monitor.MonitorID = id;
        _monitorsDict.Add(id, monitor);
    }

    public void RemoveMonitor(int id)
    {
        if (_monitorsDict.ContainsKey(id))
        {
            MonitorSource spawned = _monitorsDict.ContainsKey(id) ? _monitorsDict[id] : null;
            if (Application.isPlaying && spawned != null)
            {
                Destroy(spawned.gameObject);
            }
            if(!Application.isPlaying && spawned != null)
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
        for (int i = 0; i < monitors.Length; i++)
        {
            _monitorsDict.Add(monitors[i].MonitorID, monitors[i]);
        }
        
    }
}
