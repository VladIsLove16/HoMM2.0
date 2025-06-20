using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI HealthText;
    [SerializeField] Image HealthImage;
    [SerializeField] bool HideOnFullHP;
    public void SetRatio(float ratio)
    {
        ToggleFullHPVision(ratio);
        HealthImage.fillAmount = ratio;
    }
    public void Init()
    {
        SetRatio(1f);
    }

    private void ToggleFullHPVision(float ratio)
    {
        if (HideOnFullHP)
        {
            if (ratio != 1)
                Show();
            else
                Hide();
        }
    }

    private void Hide()
    {
        HealthImage.gameObject.SetActive(false);
    }
    private void Show()
    {
        HealthImage.gameObject.SetActive(true);
    }

  
}
