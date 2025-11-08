using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSingleStatPanel : MonoBehaviour
{
    [SerializeField] Image Image;
    [SerializeField] TextMeshProUGUI StatText;
    [SerializeField] TextMeshProUGUI StatAmount;
    public void SetInfo(Sprite sprite, string statText, string amount)
    {
        if (Image != null)
            Image.sprite = sprite;

        if (StatText != null)
            StatText.text = statText ?? string.Empty;

        if (StatAmount != null)
            StatAmount.text = amount ?? string.Empty;
    }
}
