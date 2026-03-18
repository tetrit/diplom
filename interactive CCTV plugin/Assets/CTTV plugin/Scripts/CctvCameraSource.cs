using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CctvCameraSource : MonoBehaviour
{
    [Header("Render target")]
    [SerializeField] private int width = 640;
    [SerializeField] private int height = 640;
    [SerializeField] private int depth = 24;
    [SerializeField] private RenderTextureFormat format = RenderTextureFormat.ARGB32;
    [SerializeField] private string textureNamePrefix = "RT_CCTV_";

    private Camera cam;
    private RenderTexture outputTexture;

    public Camera SourceCamera => cam;
    public RenderTexture OutputTexture => outputTexture;
    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        EnsureRenderTexture();
    }

    private void OnEnable()
    {
        EnsureRenderTexture();
    }

    private void EnsureRenderTexture()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        bool needCreate = outputTexture == null
                          || outputTexture.width != width
                          || outputTexture.height != height;

        if (!needCreate)
        {
            cam.targetTexture = outputTexture;
            return;
        }

        ReleaseTexture();

        outputTexture = new RenderTexture(width, height, depth, format);
        outputTexture.name = textureNamePrefix + gameObject.name;
        outputTexture.Create();

        cam.targetTexture = outputTexture;
    }

    private void OnDisable()
    {
        if (cam != null && cam.targetTexture == outputTexture)
            cam.targetTexture = null;
    }

    private void OnDestroy()
    {
        ReleaseTexture();
    }

    private void ReleaseTexture()
    {
        if (outputTexture == null)
            return;

        if (cam != null && cam.targetTexture == outputTexture)
            cam.targetTexture = null;

        outputTexture.Release();
        Destroy(outputTexture);
        outputTexture = null;
    }
}