using Surveillance.Monitors;
using UnityEngine;

public class MonitorSource : MonoBehaviour
{
    [SerializeField]private VirtualMonitorController  virtualMonitorController;
    [SerializeField]private YoloOverlayCanvas yoloOverlayCanvas;
    [SerializeField]private YoloRunner yoloRunner;
    
    [Header("Settings")]
    [SerializeField]public int monitorID;
    [SerializeField]public VirtualMonitorProfileSO virtualMonitorProfileSO;
    [Range(1, 15)][SerializeField] public int max_boxes;
    [SerializeField] public Color BoxColor =  Color.green;
    [Range(0.1f, 1f)][SerializeField] public float ConfidenceThreshold = 0.5f;
    


    public void ApplySettings()
    {
        virtualMonitorController.SetCameraId(monitorID);
        virtualMonitorController.Profile = virtualMonitorProfileSO;
        
        yoloOverlayCanvas.MaxBoxes = max_boxes;
        yoloOverlayCanvas.DefaultBoxColor = BoxColor;
        
        yoloRunner.CameraId = monitorID;
        yoloRunner.ConfidenceThreshold = ConfidenceThreshold;
    }

    void Start()
    {
        ApplySettings();
    }
}