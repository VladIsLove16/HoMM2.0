using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;
public class GameGridModel
{
    public Action<UnitModelCreatedParams> OnCellContentAdded;
    public Action<UnitModelRemovedParams> OnCellContentRemoved;

    [Inject] private UnitModelFactory _unitModelFactory;
    private GridXZ<GameGridCell> _grid;
    public void InitializeGrid(int width, int height)
    {
        _grid = new GridXZ<GameGridCell>(width, height, CreateEmptyGameGridObject);
    }
    public bool IsInBounds(int x, int y) => _grid.IsInBounds(x, y);

    private GameGridCell CreateEmptyGameGridObject(GridXZ<GameGridCell> grid, int x, int y)
    {
        return new GameGridCell(grid, x, y);
    }
    public bool TryGetCell(int x, int y, out IGridCell cell)
    {
        bool resutlt = _grid.TryGetGridObject(x, y, out GameGridCell gameGridCellell);
        cell = gameGridCellell;
        return resutlt;
    }
    public IGridCell GetCell(Vector2Int vector2Int)
    {
        return GetCell(vector2Int.x, vector2Int.y);
    }
    public IGridCell GetCell(int x, int y)
    {
        if (!_grid.IsInBounds(x, y))
            throw new ArgumentOutOfRangeException($"Cell ({x},{y}) is out of bounds");

        var cell = _grid.GetGridObject(x, y);
        if (cell == null)
            throw new NullReferenceException($"Cell ({x},{y}) is null in _grid");

        return cell;
    }
    public IReadOnlyList<IGridCell> GetAllCells()
    {
        return _grid
            .GetGridArray()
            .Cast<IGridCell>()
            .ToList()
            .AsReadOnly();
    }

    public bool IsEmpty(int x, int y)
    {
        return TryGetCell(x, y, out var cell) && cell.IsEmpty;
    }

    public int GetWidth()
    {
        return _grid.GetWidth();
    }

    public int GetHeight()
    {
        return _grid.GetHeight();
    }

    /// <summary>
    /// Спавнит контент заданного типа в ячейке.
    /// </summary>
    public OperationResult SpawnUnit(UnitSpawnParams unitSpawnParams) 
    {
        if (!IsInBounds(unitSpawnParams.X, unitSpawnParams.Y))
            return new OperationResult(false,$"Cell ({unitSpawnParams.X},{unitSpawnParams.Y}) out of bounds");

        if (!IsEmpty(unitSpawnParams.X, unitSpawnParams.Y))
            return new OperationResult(false,$"Cell ({unitSpawnParams.X},{unitSpawnParams.Y}) is not empty");
        // Создаём контент через выбранную фабрику
        UnitModel unit = _unitModelFactory.Create(unitSpawnParams);

        var cell = (GameGridCell)GetCell(unitSpawnParams.X, unitSpawnParams.Y);
        AddContent(cell,unit);

        return new OperationResult(true);
    }

    /// <summary>
    /// Убирает контент из клетки, если он там есть.
    /// </summary>
    public bool TryRemoveUnit(int x, int y)
    {
        var cell = (GameGridCell)GetCell(x, y);
        if (!cell.ContainsUnit())
            return false;

        cell.RemoveUnit();
        UnitModel unitModel = cell.GetUnit();
        OnCellContentRemoved?.Invoke(new UnitModelRemovedParams(unitModel));
        return true;
    }
    public (int x, int y) GetEmpty()
    {
        IGridCell gameGridCell = GetAllCells().First(x => x.IsEmpty);
        return (gameGridCell.X, gameGridCell.Y);
    }

    public void ClearGrid()
    {
        foreach (var cell in GetAllCells().Cast<GameGridCell>())
        {
            var unit = cell.GetUnit();
            if(unit == null)
                continue;
            cell.Clear();  // приватный метод удаления всего
            OnCellContentRemoved?.Invoke(new(unit));
        }
        Debug.Log("Grid Cleared");
    }

    /// <summary>Снимок всего содержимого сетки</summary>
    public GridMemento TakeSnapshot()
    {
        var all = GetAllCells() as IReadOnlyList<IGridCell>;
        return new GridMemento(all);
    }

    /// <summary>
    /// Восстановление из снапшота
    /// </summary>
    public void RestoreSnapshot(GridMemento memento)
    {
        ClearGrid();

        foreach (var cell in memento.CellStates)
        {
            foreach(var content in cell.Contents)
            {
                AddContent((GameGridCell)cell,content);
            }
        }
    }

    private void AddContent(GameGridCell gameGridCell, IGridContent content)
    {
        gameGridCell.AddContent(content);
        Debug.Log("content Added");
        UnitModelCreatedParams unitModelCreatedParams = new UnitModelCreatedParams(content as UnitModel);
        OnCellContentAdded?.Invoke(unitModelCreatedParams);
    }

    private void RemoveContent(GameGridCell gameGridCell, IGridContent content)
    {
        gameGridCell.RemoveContent(content);
        UnitModelRemovedParams unitModelRemovedParams = new UnitModelRemovedParams(content as UnitModel);
        OnCellContentRemoved?.Invoke(unitModelRemovedParams);
    }
}
