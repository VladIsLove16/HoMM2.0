using UnityEngine;

public class ActionContext
{
    public Vector2Int TargetCell;
    public IDamagable TargetObject;
    public SpellData AbilityUsed; // nullable
    public override string ToString()
    {
        return TargetObject.ToString() + " " + TargetCell.ToString() + AbilityUsed == null ? " no spell" : AbilityUsed.ToString();
    }
    //public bool IsRangedAttack;
    //public List<Vector2Int> PlannedRoute;
}
