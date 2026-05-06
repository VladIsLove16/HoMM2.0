using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSingleStatPanel : MonoBehaviour
{
    [SerializeField] Image Image;
    [SerializeField] TextMeshProUGUI StatText;
    [SerializeField] TextMeshProUGUI StatAmount;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetInfo(Sprite sprite, string statText, string amount)
    {
        ResolveReferences();

        if (Image != null)
            Image.sprite = sprite;

        if (StatText != null)
            StatText.text = statText ?? string.Empty;

        if (StatAmount != null)
            StatAmount.text = amount ?? string.Empty;
    }

    private void ResolveReferences()
    {
        if (Image == null)
        {
            Image = GetComponentInChildren<Image>(true);
        }

        if (StatText != null && StatAmount != null)
            return;

        var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            if (text == null)
                continue;

            var lowerName = text.name.ToLowerInvariant();
            if (StatAmount == null && (lowerName.Contains("amount") || lowerName.Contains("value")))
            {
                StatAmount = text;
                continue;
            }

            if (StatText == null)
            {
                StatText = text;
            }
        }

        if (StatAmount == null && texts.Length > 1)
        {
            StatAmount = texts[1];
        }
    }
}
