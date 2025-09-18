using System.Collections.Generic;
using UnityEngine;

public struct ActionPreview
{
    public List<Vector2Int> MoveRoute;
    public List<Vector2Int> InaccessibleRoute;
    public List<Vector2Int> ReachableCells;
    public bool IsActionAvailable;

    // Optional combat info
    public DamageContext Damage;
}



