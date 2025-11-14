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

    /// <summary>
    /// Allows UI layers (e.g., portrait hovers) to override the hovered cell highlight.
    /// Pass <c>null</c> to release the override and resume normal cursor hover.
    /// </summary>
    void HandleCellHovered(Vector2Int cell, Vector2Int nearestCell);
    void HandleCellActionPerformed(Vector2Int cell, Vector2Int nearestCell);
    public void SetCell(UnitViewModel unitViewModel, Vector2Int cell);
}
