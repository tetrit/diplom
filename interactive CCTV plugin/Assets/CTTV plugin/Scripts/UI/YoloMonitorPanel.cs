using UnityEngine;
using UnityEngine.UI;

public class YoloMonitorPanel : MonoBehaviour
{
    [SerializeField] private RawImage videoImage;
    [SerializeField] private RenderTexture sourceTexture;

    private void Awake()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    public void SetSourceTexture(RenderTexture texture)
    {
        sourceTexture = texture;
        Apply();
    }

    private void Apply()
    {
        if (videoImage == null)
            return;

        videoImage.texture = sourceTexture;
    }
}
