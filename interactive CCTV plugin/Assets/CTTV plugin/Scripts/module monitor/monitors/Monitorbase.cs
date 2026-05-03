using TMPro;
using UnityEngine;

public class Monitorbase : MonitorSource
{
    [SerializeField]private TextMeshProUGUI text;

    public override void ApplySettings()
    {
        base.ApplySettings();
        if (_boundCamera != null)
        {
            text.text = "cam: " + _boundCamera.gameObject.name;
        }
    }
}
