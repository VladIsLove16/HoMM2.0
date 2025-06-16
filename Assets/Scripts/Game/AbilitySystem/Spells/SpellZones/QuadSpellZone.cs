using System.Collections.Generic;
using UnityEngine;

public class QuadSpellZone : ISpellZone
{
    public List<Vector2Int> GetCells()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 1),
        };
    }
}
