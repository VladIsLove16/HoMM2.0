using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
public enum AttackType
{
    ranged,
    melee,
    magical
}
public class VisualHintSystem : MonoBehaviour
{
    [Inject] private IGridCellRenderer _gridCellRenderer;
    [Inject] private IAttackActionPanel _attackActionPanel;
    [Inject] private Dictionary<AttackType,Sprite > attackIcons;

    public void ShowRoute(List<Vector2Int> accessible, List<Vector2Int> inaccessible)
    {
        foreach (var pos in accessible)
            _gridCellRenderer.AddState(pos, CellState.accessibleRoutePoint);

        foreach (var pos in inaccessible)
            _gridCellRenderer.AddState(pos, CellState.inaccessibleRoutePoint);
    }

    public void ShowAttackHint(Vector2Int targetCell, int predictedDamage, AttackType attackType, string description = null)
    {
        var info = new AttackPreviewInfo
        {
            TargetPosition = targetCell,
            PredictedDamage = predictedDamage,
            Icon = attackIcons[attackType],
            Description = description
        };

        _attackActionPanel.Show(info);
        _gridCellRenderer.AddState(targetCell, CellState.attackTarget);
    }
}

public class AttackPreviewInfo
{
    public Vector2Int TargetPosition;
    public int PredictedDamage;
    public Sprite Icon; // меч, лук, молния, и т.п.
    public string Description;
}

public interface IAttackActionPanel
{
    void Show(AttackPreviewInfo info);
    void Hide();
}

public class AttackActionPanel : MonoBehaviour, IAttackActionPanel
{
    [SerializeField] private TMPro.TextMeshProUGUI damageText;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private UnityEngine.UI.Image icon;

    public void Show(AttackPreviewInfo info)
    {
        panelRoot.SetActive(true);
        damageText.text = $"Damage: {info.PredictedDamage}";
        icon.sprite = info.Icon;
        // Можно добавить отображение статуса, дебаффов и т.п.
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
