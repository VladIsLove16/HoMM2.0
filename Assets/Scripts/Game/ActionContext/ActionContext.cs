using System;
using Unity.Netcode;
using UnityEngine;
[Serializable]
public struct ActionContext 
{
    public Vector2Int FromCell;
    public Vector2Int TargetCell;
    public Vector2Int AttackFromCell;
    public SpellType AbilityUsed;
    public ActionContext(Vector2Int fromCell, Vector2Int targetCell, SpellType abilityUsed, Vector2Int attackFromCell)
    {
        FromCell = fromCell;
        TargetCell = targetCell;
        AbilityUsed = default;
        AttackFromCell = attackFromCell;
    }
    public ActionContext(ActionContext actionContext)
    {
        FromCell = actionContext.FromCell;
        TargetCell = actionContext.TargetCell;
        AbilityUsed = default;
        AttackFromCell = actionContext.AttackFromCell;
    }

    public override string ToString()
    {
        return " FromCell " + FromCell + " to " + TargetCell + AbilityUsed == null ? " no spell " : " with spell" ;
    }
}