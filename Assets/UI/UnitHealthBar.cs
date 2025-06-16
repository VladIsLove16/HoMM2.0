using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI HealthText;
    [SerializeField] Image HealthImage;
    public void SetRatio(float ratio)
    {
        if (ratio != 1)
            Show();
        else
            Hide();
        HealthImage.fillAmount = ratio;
    }

    internal void Hide()
    {
        HealthImage.gameObject.SetActive(false);
    }
    internal void Show()
    {
        HealthImage.gameObject.SetActive(true);
    }

    internal void Init()
    {
        SetRatio(1f);
    }
}
