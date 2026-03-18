using System.Collections.Generic;
using UnityEngine;

public class YoloOverlayCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private YoloBoxUI boxPrefab;
    [SerializeField] private int maxBoxes = 30;
    [SerializeField] private Color defaultBoxColor = Color.green;

    private readonly List<YoloBoxUI> boxes = new();

    private void Awake()
    {
        if (canvasRect == null)
            canvasRect = GetComponent<RectTransform>();

        CreatePool();
    }

    private void CreatePool()
    {
        boxes.Clear();

        for (int i = 0; i < maxBoxes; i++)
        {
            YoloBoxUI box = Instantiate(boxPrefab, canvasRect);
            box.Hide();
            boxes.Add(box);
        }
    }

    public void ClearBoxes()
    {
        for (int i = 0; i < boxes.Count; i++)
            boxes[i].Hide();
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
        if (index < 0 || index >= boxes.Count)
            return;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float scaleX = canvasWidth / inputWidth;
        float scaleY = canvasHeight / inputHeight;

        float left = x1 * scaleX;
        float right = x2 * scaleX;
        float top = y1 * scaleY;
        float bottom = y2 * scaleY;

        float width = right - left;
        float height = bottom - top;

        if (width <= 1f || height <= 1f)
            return;

        float centerX = left + width * 0.5f;
        float centerY = top + height * 0.5f;

        // Перевод из координат изображения:
        // (0,0) в левом верхнем углу
        // в координаты UI:
        // (0,0) в центре canvas, Y направлена вверх
        float anchoredX = centerX - canvasWidth * 0.5f;
        float anchoredY = -(centerY - canvasHeight * 0.5f);

        boxes[index].SetBox(
            anchoredX,
            anchoredY,
            width,
            height,
            className,
            confidence,
            color
        );
    }
}