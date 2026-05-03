using TMPro;
using UnityEngine;

public class Monitorbase : MonitorSource
{
    [SerializeField]private TextMeshProUGUI text;


    protected override void UpdateCameraTexture()
    {
        base.UpdateCameraTexture();
        if (_boundCamera != null)
        {
            text.text = "cam: " + _boundCamera.gameObject.name;
        }
    }
}
