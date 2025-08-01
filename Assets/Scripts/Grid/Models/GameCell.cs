using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameCell : IGridCell
{
    private readonly List<IGridContent> _contents = new();
    private GridXZ<GameCell> _grid;
    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector2Int Position => new(X, Y);
    public bool IsEmpty => !_contents.Any();
    public IReadOnlyList<IGridContent> Contents => _contents;
    public UnitModel Unit => _contents.OfType<UnitModel>().FirstOrDefault();

    public GameCell(GridXZ<GameCell> grid, int x, int y)
    {
        _grid = grid;
        X = x; Y = y;
    }

    public void AddContent(IGridContent c)
    {
        if (c == null) 
            return;
        _contents.Add(c);
        _grid.TriggerGridObjectChanged(this);
    }

    public void RemoveContent(IGridContent c)
    {
        _contents.Remove(c);
        _grid.TriggerGridObjectChanged(this);
    }

    public void TryRemoveUnit(IGridContent c)
    {
        if(Unit!=null)
        {
            RemoveContent(Unit);
        }
    }

    public void Clear()
    {
        foreach (var c in _contents.ToList())
            RemoveContent(c);
    }

    public bool CanMove() =>
        _contents.OfType<IBlockable>().All(b => b.CanMoveThrough());

    public List<GameCell> GetNeighbors(int range, bool includeCorners)
    {
        var list = new List<GameCell>();
        for (int dx = -range; dx <= range; dx++)
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = X + dx, ny = Y + dy;
                if (!_grid.IsInBounds(nx, ny)) continue;
                if (Math.Abs(dx) + Math.Abs(dy) <= range || includeCorners)
                    list.Add(_grid.GetGridObject(nx, ny));
            }
        return list;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{X},{Y}");
        _contents.ForEach(c => sb.AppendLine(c.ToString()));
        return sb.ToString();
    }
}
