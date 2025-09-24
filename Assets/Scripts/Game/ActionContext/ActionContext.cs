using UnityEngine;

public class ActionContext
{
    public Vector2Int FromCell;
    public Vector2Int TargetCell;
    public SpellData AbilityUsed;
    public ActionContext(Vector2Int fromCell, Vector2Int targetCell, SpellData abilityUsed)
    {
        FromCell = fromCell;
        TargetCell = targetCell;
        AbilityUsed = abilityUsed;
    }

    public override string ToString()
    {
        return " FromCell " + FromCell + " to " + TargetCell + AbilityUsed == null ? " no spell " : " with spell" ;
    }
    //public Vector2Int? AttackFromCell; 
}
