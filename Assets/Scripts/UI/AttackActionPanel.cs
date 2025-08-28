using UnityEngine;
using Zenject;

public class AttackActionPanel : MonoBehaviour, IAttackActionPanel
{
    [SerializeField] private TMPro.TextMeshProUGUI damageText;
    [SerializeField] private GameObject panelRoot;
    public void Show(AttackPreviewInfo info)
    {
        panelRoot.SetActive(true);
        damageText.text = $"Damage: {info.DamageContext.DamageAmount } \nDied: {info.DamageContext.DieAmount}";
        // Можно добавить отображение статуса, дебаффов и т.п.
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
