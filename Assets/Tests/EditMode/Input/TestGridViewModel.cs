using System.Collections.Generic;
using UnityEngine;
using Tests.Common;
using System;

namespace Tests.EditMode.Input
{
    public partial class CellInputHandler_EditModeIntegrationTests
    {
        public class TestGridViewModel : IGridViewModel
        {
            public event System.Action<int, int> GridInited;
            public event System.Action<PreviewResult> PreviewChanged;
            public event System.Action<PreviewResult> PreviewUpdated;

            public IGridRenderSettings RenderSettings { get; set; } = new TestGridRenderSettings();
            public int Width { get; private set; }
            public int Height { get; private set; }
            public Action<UnitViewModel> UnitSpawned { get; set; }

            public Vector2Int? LastNullableHover { get; private set; }
            public Vector2Int? LastHover { get; private set; }
            public Vector2Int? LastNearestHover { get; private set; }
            public Vector2Int? LastActionCell { get; private set; }
            public Vector2Int? LastActionNearest { get; private set; }
            public UnitViewModel LastMovedUnit { get; private set; }
            public Vector2Int? LastMovedCell { get; private set; }
            public bool CanExecuteResult { get; set; } = true;
            public KeyValuePair<Vector2Int, Vector2Int>? LastSelection { get; private set; }

            public void RaisePreview(PreviewResult result) => PreviewChanged?.Invoke(result);

            public void RaisePreviewUpdate(PreviewResult result) => PreviewUpdated?.Invoke(result);

            public void SimulateGridInit(int width, int height)
            {
                Width = width;
                Height = height;
                GridInited?.Invoke(width, height);
            }

            public void HandleCellHovered(Vector2Int? cell)
            {
                LastNullableHover = cell;
            }

            public void HandleCellHovered(Vector2Int cell, Vector2Int nearestCell)
            {
                LastHover = cell;
                LastNearestHover = nearestCell;
            }

            public void HandleCellActionPerformed(Vector2Int cell, Vector2Int nearestCell)
            {
                LastActionCell = cell;
                LastActionNearest = nearestCell;
            }

            public void HandleCellSelected(KeyValuePair<Vector2Int, Vector2Int> coords)
            {
                LastSelection = coords;
            }

            public void SetCell(UnitViewModel unitViewModel, Vector2Int cell)
            {
                LastMovedUnit = unitViewModel;
                LastMovedCell = cell;
            }

            public bool CanExecute(ActionType actionType, ActionContext actionContext)
            {
                return CanExecuteResult;
            }
        }
    }
}
