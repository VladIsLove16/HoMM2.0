using System.Collections.Generic;
using UnityEngine;
using Tests.Common;

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
                // no-op for tests
            }
        }
    }
}
