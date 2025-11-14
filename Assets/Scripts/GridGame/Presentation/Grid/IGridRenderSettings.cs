using System.Collections.Generic;

public interface IGridRenderSettings
{
    float CellSize { get; }
    float CellHeight { get; }
    float CellPadding { get; }
    IReadOnlyList<CellMaterial> Materials { get; }
}
