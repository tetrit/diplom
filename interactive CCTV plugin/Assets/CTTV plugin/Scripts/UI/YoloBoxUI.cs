using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class YoloBoxUI : MonoBehaviour
{
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text label;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (frameImage == null)
            frameImage = GetComponent<Image>();
    }

    public void SetBox(
        float x,
        float y,
        float width,
        float height,
        string className,
        float confidence,
        Color color)
    {
        rectTransform.anchoredPosition = new Vector2(x, y);
        rectTransform.sizeDelta = new Vector2(width, height);

        if (frameImage != null)
            frameImage.color = color;

        if (label != null)
        {
            label.text = $"{className} {confidence:F2}";
            label.color = color;
            label.rectTransform.anchoredPosition = new Vector2(0, height * 0.5f + 12f);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}