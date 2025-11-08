    using System.Collections.Generic;

public interface IGridCell
{
    int X { get; }
    int Y { get; }
    bool IsEmpty { get; }
    IReadOnlyList<IGridContent> Contents { get; }
    UnitModel Unit { get; }
}
