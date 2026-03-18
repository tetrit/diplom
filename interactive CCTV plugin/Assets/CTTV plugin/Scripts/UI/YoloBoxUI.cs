using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class YoloBoxUI : MonoBehaviour
{
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Vector2 labelOffset = new Vector2(4f, 4f);

    private RectTransform rectTransform;
    private RectTransform labelRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (frameImage == null)
            frameImage = GetComponent<Image>();

        if (label != null)
            labelRect = label.rectTransform;
    }

    public void SetNormalizedBox(
        float leftNorm,
        float topNorm,
        float rightNorm,
        float bottomNorm,
        string className,
        float confidence,
        Color color)
    {
        leftNorm = Mathf.Clamp01(leftNorm);
        rightNorm = Mathf.Clamp01(rightNorm);
        topNorm = Mathf.Clamp01(topNorm);
        bottomNorm = Mathf.Clamp01(bottomNorm);

        if (rightNorm <= leftNorm || bottomNorm <= topNorm)
        {
            Hide();
            return;
        }


        rectTransform.anchorMin = new Vector2(leftNorm, 1f - bottomNorm);
        rectTransform.anchorMax = new Vector2(rightNorm, 1f - topNorm);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        if (frameImage != null)
            frameImage.color = color;

        if (label != null)
        {
            label.text = $"{className} {confidence:F2}";
            label.color = color;

            if (labelRect != null)
            {
                //labelRect.anchorMin = new Vector2(0f, 1f);
                //labelRect.anchorMax = new Vector2(0f, 1f);
                //labelRect.pivot = new Vector2(0f, 0f);
                //labelRect.anchoredPosition = labelOffset;
            }
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
