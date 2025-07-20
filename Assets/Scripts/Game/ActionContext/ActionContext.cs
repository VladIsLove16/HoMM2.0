using UnityEngine;

public class ActionContext
{
    public UnitViewModel Unit;
    public Vector2Int TargetCell;
    public IDamagable TargetUnit;
    public SpellData AbilityUsed; // nullable
    //public bool IsRangedAttack;
    //public List<Vector2Int> PlannedRoute;
}
