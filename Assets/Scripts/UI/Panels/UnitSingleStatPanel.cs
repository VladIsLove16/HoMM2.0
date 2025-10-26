using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSingleStatPanel : MonoBehaviour
{
    [SerializeField] Image Image;
    [SerializeField] TextMeshProUGUI StatText;
    [SerializeField] TextMeshProUGUI StatAmount;
    public void SetInfo(Sprite sprite,string statText,int amount)
    {
        Image.sprite = sprite;
        StatText.text = statText;
        StatAmount.text = amount.ToString();
    }
}
