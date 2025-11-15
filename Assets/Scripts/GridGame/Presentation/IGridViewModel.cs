using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridViewModel
{
    event Action<int, int> GridInited;
    event Action<PreviewResult> PreviewChanged;
    event Action<PreviewResult> PreviewUpdated;

    int Width { get; }
    int Height { get; }
    IGridRenderSettings RenderSettings { get; set; }
    Action<UnitViewModel> UnitSpawned { get; set; }

    void HandleCellHovered(Vector2Int? cell);
    void HandleCellHovered(Vector2Int cell, Vector2Int nearestCell);
    void HandleCellSelected(KeyValuePair<Vector2Int, Vector2Int> coords);
    void HandleCellActionPerformed(Vector2Int cell, Vector2Int nearestCell);
    void SetCell(UnitViewModel unitViewModel, Vector2Int cell);
    bool CanExecute(ActionType actionType, ActionContext actionContext);
}
