using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridCellRenderer
{
    void Clear();
    void Render(int width, int height, float cellSize, Vector3 origin, float padding);
    CellState[] GetCellStates(Vector2Int coords);
    void AddState(Vector2Int coords, CellState state);
    void AddStates(List<Vector2Int> points, CellState state);
    void RemoveStates(CellState state);
    void RemoveState(Vector2Int hoveredCell, CellState state);
}
