// GridMemento.cs
using System.Collections.Generic;
using System.Linq;

public class GridMemento
{
    public readonly IReadOnlyList<IGridCell> CellStates;
    public GridMemento(IReadOnlyList<IGridCell> cellStates)
    {
        CellStates = cellStates.ToList();
    }
}
