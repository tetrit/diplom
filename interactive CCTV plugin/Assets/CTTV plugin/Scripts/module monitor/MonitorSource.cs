using Surveillance.Monitors;
using UnityEngine;

public class MonitorSource : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private int monitorID;
    
    [Header("Настройки")]
    [SerializeField] private int targetCameraId;[SerializeField] private VirtualMonitorProfileSO profile;


    [SerializeField] private VirtualMonitorController _controller;

    public int MonitorID
    {
        get { return monitorID; }
        set
        {
            monitorID = value;
            ApplySettings();
        }
    }

    public int TargetCameraId
    {
        get { return targetCameraId; }
        set
        {
            targetCameraId = value;
            ApplySettings();
        }
    }

    private void Awake()
    {
        if (_controller == null)
            _controller = GetComponentInChildren<VirtualMonitorController>();
    }

    private void Start()
    {
        ApplySettings();
    }

    public void ApplyProfile(VirtualMonitorProfileSO newProfile)
    {
        profile = newProfile;
        ApplySettings();
    }

    public void ApplySettings()
    {

        if (_controller == null)
            _controller = GetComponentInChildren<VirtualMonitorController>();


        if (_controller != null)
        {
            _controller.Initialize(targetCameraId, profile);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Компонент VirtualMonitorController не найден! Добавьте его на префаб монитора.");
        }
    }
}