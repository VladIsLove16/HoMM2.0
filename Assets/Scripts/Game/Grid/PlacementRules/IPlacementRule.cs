using UnityEngine;
public abstract class PlacementRuleData : ScriptableObject
{
    public abstract bool CanPlaceTogether(IGridContent content1, IGridContent content2);
}