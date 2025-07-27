using UnityEngine;
using Zenject;

public class AttackActionPanel : MonoBehaviour, IAttackActionPanel
{
    [SerializeField] private TMPro.TextMeshProUGUI damageText;
    [SerializeField] private GameObject panelRoot;
    [Inject] CursorService cursorService;
    public void Show(AttackPreviewInfo info)
    {
        panelRoot.SetActive(true);
        damageText.text = $"Damage: {info.PredictedDamage}";
        cursorService.SetCursorState(info.CursorState);
        // Можно добавить отображение статуса, дебаффов и т.п.
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
