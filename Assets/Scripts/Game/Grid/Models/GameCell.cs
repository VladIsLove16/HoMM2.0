using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public interface IGridCell
{
    int X { get; }
    int Y { get; }
    bool IsEmpty { get; }
    IReadOnlyList<IGridContent> Contents { get; }

    UnitModel Unit { get; }
}

public class GameCell : IGridCell
{
    protected GridXZ<GameCell> mainGrid;
    private List<IGridContent> _gameGridObjectContents = new List<IGridContent>();
    public int x;
    public int y;
    public Vector2Int Position => new Vector2Int(x, y);

    public int X => x;
    public int Y => y;
    public UnitModel Unit => GetUnit();
    public virtual bool IsEmpty
    {
        get
        {
           return !_gameGridObjectContents.Any();
        }
    }

    public IReadOnlyList<IGridContent> Contents => _gameGridObjectContents as IReadOnlyList<IGridContent>;
    public GameCell(GridXZ<GameCell> grid, int x, int y)
    {
        this.mainGrid = (grid);
        this.x = x;
        this.y = y;
    }
    public void Setup(GridXZ<GameCell> grid, int x, int y)
    {
        this.mainGrid = (grid);
        this.x = x;
        this.y = y;
    }
    public void AddContent(IGridContent content)
    {
        if (content == null)
        {
            Debug.Log("null content have not added");
            return;
        }
        Debug.Log($"content {content.UnitType} added to {X} {Y}");

        _gameGridObjectContents.Add(content);
        content.SetCoords(X, Y);

        mainGrid.TriggerGridObjectChanged(new(content.X, content.Y, content, null));

    }

    public void RemoveContent(IGridContent content)
    {
        _gameGridObjectContents.Remove(content);
        mainGrid.TriggerGridObjectChanged(new(content.X, content.Y, null, content));
    }

    public void Clear()
    {
        var content = _gameGridObjectContents.ToList();
        foreach(var contentObj in content)
        {
            RemoveContent(contentObj);
        }
    }

    public bool CanMove()
    {
        foreach (IGridContent content in _gameGridObjectContents)
        {
            if (content is IBlockable blockable && !blockable.CanMoveThrough())
            {
                return false; // Если хотя бы один объект не пропускает движение
            }
        }
        return true; // Если все объекты позволяют двигаться через клетку
    }
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(x + " " + y);
        foreach(var content in _gameGridObjectContents)
        {
            stringBuilder.AppendLine(content.ToString());
        }
        return stringBuilder.ToString();
    }
    public List<GameCell> GetNeigbours(int range,bool AddCorners)
    {
        List<GameCell> neighbors = new List<GameCell>();
        for (int i = x - range; i <= x + range; i++)
        {
            for (int j = y - range; j <= y + range; j++)
            {
                // Проверяем, что координаты находятся в пределах матрицы
                if (i >= 0 && i < mainGrid.GetHeight() && j >= 0 && j < mainGrid.GetWidth())
                {
                    if((Math.Abs(x-i) + Math.Abs(y-j)) <= range || AddCorners)
                        neighbors.Add(mainGrid.GetGridObject(i,j));
                }
            }
        }
        Debug.Log("Neigbour Count" +  neighbors.Count);
        return neighbors;
    }

    public bool ContainsUnit()
    {
        foreach(var  cell in _gameGridObjectContents)
        {
           if (cell is UnitModel)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveUnit()
    {
        var gameGridObjectContents = _gameGridObjectContents.ToList();
        foreach (var cell in gameGridObjectContents)
        {
            if (cell is UnitModel unit)
            {
                _gameGridObjectContents.Remove(unit);
                mainGrid.TriggerGridObjectChanged(new GridXZ<GameCell>.GridObjectChangedEventArgs(unit.X, unit.Y, null, unit));
                return;
            }
        }
    }

    public UnitModel GetUnit()
    {
        foreach (var cell in _gameGridObjectContents)
        {
            if (cell is UnitModel unit)
            {
                return unit;
            }
        }
        return null;
    }
}
