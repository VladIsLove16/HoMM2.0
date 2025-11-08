using System.Collections.Generic;
using UnityEngine;

public interface IWorldToCellProvider
{
    Vector3 ToWorld(int x, int y);
    bool ToGrid(Vector3 position, out Vector2Int coords);
    bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int,Vector2Int> coords);
}