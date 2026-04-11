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
    
    /*TODO: переделать это нахуй. Надо чтобы просто был поиск по сценам, а то это словарь всё время обнуляется да и в целом
     будет лучше да и проще в теории, ну ты понял что я из прошлого имел ввиду, да?
     
     TODO: сделай ещё bootstraper, который будет сам манагер назначать
    */
    [SerializeField]private Dictionary<int, VirtualCameraSource> virtualcameraDict = new Dictionary<int, VirtualCameraSource>();


    [SerializeField]private int cameraIDToRemove;

    public int CameraIDToRemove
    {
        get { return cameraIDToRemove; }
    }
    public event Action<VirtualCameraSource> cameraInitializedEvent;
    public event Action<int> cameraDestroyedEvent;
    void Start()
    {
        //SpawnCamera(_cameraPrefab);
    }
    

    private void SpawnCamera(VirtualCameraSource virtualcamera)
    {
        VirtualCameraSource spawned = Instantiate(virtualcamera);
        spawned.CameraId = virtualcameraDict.Count;
        spawned.name = "CCTV cam_" + spawned.CameraId;
        
        spawned.Initialize();
        
        AddCameraToDict(spawned.CameraId, spawned);
        cameraInitializedEvent?.Invoke(spawned);
        Debug.Log("Camera Spawned: " + spawned.CameraId);
        
    }

    private void DestroyCamera(int cameraID)
    {
        VirtualCameraSource spawned = virtualcameraDict.ContainsKey(cameraID) ? virtualcameraDict[cameraID] : null;
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
        if (cameraId >= 0 && cameraId < virtualcameraDict.Count &&  virtualcameraDict[cameraId] != null)
        {
            return true;
        }
        return false;
    }

    public VirtualCameraSource GetVirtualCamera(int cameraId)
    {
        if (TryGetVirtualCamera(cameraId))
        {
            return virtualcameraDict[cameraId];
        }

        return null;
    }
    
    
    private void AddCameraToDict(int cameraID, VirtualCameraSource virtualcamera)
    {
        virtualcameraDict.Add(cameraID, virtualcamera);
    }

    private void RemoveCameraFromDict(int cameraID)
    {
        virtualcameraDict.Remove(cameraID);
    }
    
    
}
