using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;
public class GameModel
{
    public Action<UnitModelCreatedParams> OnCellContentAdded;
    public Action<UnitModelRemovedParams> OnCellContentRemoved;
    public Action<UnitModelsSwapped> OnCellContentSwaped;
    public Action<UnitModelLegacy, Vector2Int> OnCellContentMoved;
    public Action<UnitModelLegacy> OnTurnStarted;

    [Inject] private UnitModelFactory _unitModelFactory;
    private GridXZ<GameCell> _grid;
    private MovementSystem _movementSystem;


    public void InitializeGrid(int width, int height)
    {
        _grid = new GridXZ<GameCell>(width, height, CreateEmptyGameGridObject);
        _movementSystem = new MovementSystem(_grid);
        Debug.Log("_grid created");
    }
    public bool IsInBounds(int x, int y) => _grid.IsInBounds(x, y);

    private GameCell CreateEmptyGameGridObject(GridXZ<GameCell> grid, int x, int y)
    {
        return new GameCell(grid, x, y);
    }
    public bool TryGetCell(int x, int y, out IGridCell cell)
    {
        bool resutlt = _grid.TryGetGridObject(x, y, out GameCell gameGridCellell);
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
        UnitModelLegacy unit = _unitModelFactory.Create(unitSpawnParams);
        unit.OnTurnStart +=  () => OnTurnStarted?.Invoke(unit);

        var cell = (GameCell)GetCell(unitSpawnParams.X, unitSpawnParams.Y);
        AddContent(cell,unit);

        return new OperationResult(true);
    }

    /// <summary>
    /// Убирает контент из клетки, если он там есть.
    /// </summary>
    public bool TryRemoveUnit(int x, int y)
    {
        var cell = (GameCell)GetCell(x, y);
        if (!cell.ContainsUnit())
            return false;

        cell.RemoveUnit();
        UnitModelLegacy unitModel = cell.GetUnit();
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
        foreach (var cell in GetAllCells().Cast<GameCell>())
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
                AddContent((GameCell)cell,content);
            }
        }
    }

    private void AddContent(GameCell gameGridCell, IGridContent content)
    {
        gameGridCell.AddContent(content);
        Debug.Log("content Added");
        UnitModelCreatedParams unitModelCreatedParams = new UnitModelCreatedParams(content as UnitModelLegacy);
        OnCellContentAdded?.Invoke(unitModelCreatedParams);
    }

    private void RemoveContent(GameCell gameGridCell, IGridContent content)
    {
        gameGridCell.RemoveContent(content);
        UnitModelRemovedParams unitModelRemovedParams = new UnitModelRemovedParams(content as UnitModelLegacy);
        OnCellContentRemoved?.Invoke(unitModelRemovedParams);
    }

    internal bool SwapUnits(Vector2Int to, Vector2Int from)
    {
        if(CanSwap(to, from))
        {
            GameCell fromCell = (GameCell) GetCell(from);
            GameCell toCell = (GameCell) GetCell(to);
            UnitModelLegacy fromUnit = fromCell.GetUnit();
            UnitModelLegacy toUnit = toCell.GetUnit();
            if (fromUnit != null && toUnit != null)
            {
                fromCell.RemoveUnit();
                toCell.RemoveUnit();
                fromCell.AddContent(toUnit);
                toCell.AddContent(fromUnit);
                OnCellContentSwaped?.Invoke(new UnitModelsSwapped(toUnit, fromUnit));
                return true;
            }
        }
        return false;
    }

    private bool CanSwap(Vector2Int coords, Vector2Int selectedCellCoords)
    {
        return true;
    }

    public bool ContainsUnit(Vector2Int coords)
    {
        return ((GameCell)GetCell(coords)).ContainsUnit();
    }

    public List<Vector2Int> GetAvailableMovePoints(Vector2Int from,int speed)
    {
       return _movementSystem.GetReachableCells(from,speed);
    }

    public bool MoveUnit(Vector2Int to, Vector2Int from)
    {
        GameCell fromCell = (GameCell)GetCell(from);
        GameCell toCell = (GameCell)GetCell(to);
        UnitModelLegacy fromUnit = fromCell.GetUnit();
        if(fromUnit == null)
        {
            Debug.LogWarning("no unit in " + fromCell.x + " " + fromCell.y);
            return false;
        }
        fromCell.RemoveUnit();
        toCell.AddContent(fromUnit);
        OnCellContentMoved?.Invoke(fromUnit, to);
        return true;
    }
}
