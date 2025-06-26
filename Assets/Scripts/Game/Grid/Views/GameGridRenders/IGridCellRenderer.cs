using System;
using UnityEngine;

public interface IGridCellRenderer
{
    void Clear();
    void Render(int width, int height, float cellSize, Vector3 origin, float padding);
    Vector3 ToWorld(int x, int y);
    bool ToGrid(Vector3 position, out Vector2Int coords);
    void SetCellState(Vector2Int cellCoords, CellState v);
    CellState GetCellState(Vector2Int coords);
    void ReturnState(Vector2Int hoveredCell);
    CellState GetPrevState(Vector2Int hoveredCell);
}