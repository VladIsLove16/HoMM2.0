using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
public class GameGridModel
{
    public event EventHandler<GridCellChangedEventArgs> OnGridObjectChanged;
    private GridXZ<GameGridCell> grid;
    public void InitializeGrid(int width, int height)
    {
        grid = new GridXZ<GameGridCell>(width, height, CreateEmptyGameGridObject);
        grid.OnGridObjectChanged += (obj, args) =>
        {
            Debug.Log("grid changed " + args.x + " " + args.y);
            OnGridObjectChanged?.Invoke(obj, args);
        };
    }
    private GameGridCell CreateEmptyGameGridObject(GridXZ<GameGridCell> grid, int x, int y)
    {
        return new GameGridCell(grid, x, y);
    }
    public bool TryGetCell(int x, int y, out GameGridCell cell)
    {
        return grid.TryGetGridObject(x, y, out cell);
    }

    public GameGridCell GetCell(int x, int y)
    {
        if (!grid.IsInBounds(x, y))
            throw new ArgumentOutOfRangeException($"Cell ({x},{y}) is out of bounds");

        var cell = grid.GetGridObject(x, y);
        if (cell == null)
            throw new NullReferenceException($"Cell ({x},{y}) is null in grid");

        return cell;
    }

    public bool IsEmpty(int x, int y)
    {
        return TryGetCell(x, y, out var cell) && cell.IsEmpty();
    }
    public List<GameGridCell> GetCellModels()
    {
        GameGridCell[,] array = grid.GetGridArray();
        List<GameGridCell> list = array.Cast<GameGridCell>().ToList();
        return list;
    }

    public int GetWidth()
    {
       return grid.GetWidth();  
    }
    public int GetHeight()
    {
       return grid.GetHeight();  
    }
    private void AddContent(int x, int y, IGridContent content)
    {
        var cell = GetCell(x, y);
        cell.AddContent(content);
    }

    public bool TryAddContent(int x, int y, IGridContent content)
    {
        if (!IsEmpty(x, y))
        {
            Debug.LogWarning($"Cell ({x},{y}) already has content.");
            return false;
        }

        AddContent(x, y, content);
        return true;
    }

}
