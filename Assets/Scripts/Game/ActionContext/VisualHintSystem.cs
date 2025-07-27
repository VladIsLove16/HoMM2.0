using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
public class VisualHintSystem : MonoBehaviour
{
    private IAttackActionPanel _attackActionPanel;
    [Inject]
    public void Construct(IAttackActionPanel attackActionPanel)
    {
        _attackActionPanel = attackActionPanel;
    }
    public void ShowAttackHint(Vector2Int targetCell, DamageContext predictedDamage, CursorState cursorState, string description = null)
    {
        var info = new AttackPreviewInfo
        {
            TargetPosition = targetCell,
            PredictedDamage = predictedDamage,
            CursorState = cursorState,
            Description = description
        };

        _attackActionPanel.Show(info);
    }
}

public class AttackPreviewInfo
{
    public Vector2Int TargetPosition;
    public DamageContext PredictedDamage;
    public CursorState CursorState; // меч, лук, молния, и т.п.
    public string Description;
}
