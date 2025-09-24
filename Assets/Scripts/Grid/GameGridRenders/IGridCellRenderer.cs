using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridCellRenderer
{
    void Clear();
    void Render(int width, int height, float cellSize, Vector3 origin, float padding);
    CellState[] GetCellStates(Vector2Int coords);
    void Bind(IGridViewModel gameViewModel);
    void Unbind(IGridViewModel gameViewModel);
}
