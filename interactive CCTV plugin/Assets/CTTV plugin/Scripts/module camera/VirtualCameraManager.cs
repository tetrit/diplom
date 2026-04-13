using System;
using System.Collections.Generic;
using NUnit.Framework;
using Surveillance.Cameras;
using UnityEngine;

public class VirtualCameraManager : MonoBehaviour
{
    
    [SerializeField] private VirtualCameraSource cameraPrefab;
    public VirtualCameraSource CameraPrefab{get{return cameraPrefab;}}
    //[SerializeField]private List<VirtualCameraSource> virtualCameraList = new List<VirtualCameraSource>();
    
    /*
     
     TODO: сделай ещё bootstraper, который будет сам манагер назначать
    */
    private Dictionary<int, VirtualCameraSource> _virtualcameraDict = new Dictionary<int, VirtualCameraSource>();


    [SerializeField]private int cameraIDToRemove;

    public int CameraIDToRemove
    {
        get { return cameraIDToRemove; }
    }
    public event Action<VirtualCameraSource> cameraInitializedEvent;
    
    public event Action<int> cameraDestroyedEvent;

    

    private void SpawnCamera(VirtualCameraSource virtualcamera)
    {

        int id = 0;
        if (IsDictNull() && FindAnyObjectByType<VirtualCameraSource>() != null)
        {
            FillVirtualCameraDict();
        }

        while (_virtualcameraDict.ContainsKey(id))
        {
            id++;
        }
        
        VirtualCameraSource spawned = Instantiate(virtualcamera);
        spawned.CameraId = id;
        spawned.name = "CCTV cam_" + spawned.CameraId;
        
        spawned.Initialize();
        
        AddCameraToDict(spawned.CameraId, spawned);
        cameraInitializedEvent?.Invoke(spawned);
        Debug.Log("Camera Spawned: " + spawned.CameraId);
        
    }

    private void DestroyCamera(int cameraID)
    {
        if (IsDictNull() && FindAnyObjectByType<VirtualCameraSource>() != null)
        {
            FillVirtualCameraDict();
        }
        VirtualCameraSource spawned = _virtualcameraDict.ContainsKey(cameraID) ? _virtualcameraDict[cameraID] : null;
        if (Application.isPlaying && spawned != null)
        {
            Destroy(spawned.gameObject);
        }
        if(!Application.isPlaying && spawned != null)
        { 
            DestroyImmediate(spawned.gameObject);
        }

        RemoveCameraFromDict(cameraID);
        cameraDestroyedEvent?.Invoke(cameraID);
    }

    public void SpawnCameraEditor(VirtualCameraSource virtualcamera)
    {
        SpawnCamera(virtualcamera);

    }

    public void DestroyCameraEditor(int cameraID)
    {
        DestroyCamera(cameraID);
    }

    private bool TryGetVirtualCamera(int cameraId)
    {
        if (_virtualcameraDict.ContainsKey(cameraId))
        {

            return true;
        }

        return false;
    }

    public VirtualCameraSource GetVirtualCamera(int cameraId)
    {
        if (IsDictNull() && FindAnyObjectByType<VirtualCameraSource>() != null)
        {
            FillVirtualCameraDict();
        }
        
        if (TryGetVirtualCamera(cameraId))
        {
            return _virtualcameraDict[cameraId];
        }

        return null;
    }
    
    
    private void AddCameraToDict(int cameraID, VirtualCameraSource virtualcamera)
    {
        _virtualcameraDict.Add(cameraID, virtualcamera);
    }

    private void RemoveCameraFromDict(int cameraID)
    {
        _virtualcameraDict.Remove(cameraID);
    }

    private bool IsDictNull()
    {
        if (_virtualcameraDict.Count == 0)
        {
            return true;
        }
        return false;
    }

    private void FillVirtualCameraDict()
    {
        var cameras = FindObjectsByType<VirtualCameraSource>(FindObjectsSortMode.InstanceID);
        System.Array.Sort(cameras, (a, b) => a.CameraId.CompareTo(b.CameraId));

        for (int i = 0; i < cameras.Length; i++)
        {
            _virtualcameraDict.Add(cameras[i].CameraId, cameras[i]);
        }
    }
    
}
