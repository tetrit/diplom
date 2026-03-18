using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class YoloOverlayCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform overlayRect;
    [SerializeField] private YoloBoxUI boxPrefab;
    [SerializeField] private int initialPoolSize = 30;
    [SerializeField] private Color defaultBoxColor = Color.green;

    private readonly List<YoloBoxUI> boxes = new();
    private bool initialized;

    private void Reset()
    {
        if (overlayRect == null)
            overlayRect = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        if (overlayRect == null)
            overlayRect = GetComponent<RectTransform>();

        EnsurePoolSize(initialPoolSize);
        initialized = true;
    }

    private void EnsurePoolSize(int size)
    {
        if (boxPrefab == null || overlayRect == null)
            return;

        while (boxes.Count < size)
        {
            YoloBoxUI box = Instantiate(boxPrefab, overlayRect);
            box.Hide();
            boxes.Add(box);
        }
    }

    public void ClearBoxes()
    {
        Initialize();

        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i] != null)
                boxes[i].Hide();
        }
    }

    public void DrawBox(
        int index,
        float x1,
        float y1,
        float x2,
        float y2,
        int inputWidth,
        int inputHeight,
        string className,
        float confidence)
    {
        DrawBox(index, x1, y1, x2, y2, inputWidth, inputHeight, className, confidence, defaultBoxColor);
    }

    public void DrawBox(
        int index,
        float x1,
        float y1,
        float x2,
        float y2,
        int inputWidth,
        int inputHeight,
        string className,
        float confidence,
        Color color)
    {
        Initialize();

        if (index < 0)
            return;

        if (boxPrefab == null || overlayRect == null)
            return;

        if (inputWidth <= 0 || inputHeight <= 0)
            return;

        EnsurePoolSize(index + 1);

        float left = Mathf.Min(x1, x2) / inputWidth;
        float right = Mathf.Max(x1, x2) / inputWidth;
        float top = Mathf.Min(y1, y2) / inputHeight;
        float bottom = Mathf.Max(y1, y2) / inputHeight;

        left = Mathf.Clamp01(left);
        right = Mathf.Clamp01(right);
        top = Mathf.Clamp01(top);
        bottom = Mathf.Clamp01(bottom);

        if (right - left <= 0.001f || bottom - top <= 0.001f)
        {
            boxes[index].Hide();
            return;
        }

        boxes[index].SetNormalizedBox(
            left,
            top,
            right,
            bottom,
            className,
            confidence,
            color
        );
    }
}