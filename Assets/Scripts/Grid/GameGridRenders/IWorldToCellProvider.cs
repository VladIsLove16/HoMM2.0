using UnityEngine;

public interface IWorldToCellProvider
{
    Vector3 ToWorld(int x, int y);
    bool ToGrid(Vector3 position, out Vector2Int coords);
}