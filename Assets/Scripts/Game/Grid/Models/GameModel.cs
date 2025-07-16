using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;
public class GameModel
{
    public Action<UnitModelCreatedParams> CellContentAdded;
    public Action<UnitModelRemovedParams> CellContentRemoved;
    public Action<UnitModelsSwapped> CellContentSwaped;
    public Action<UnitModel, Vector2Int> CellContentMoved;
    public Action<UnitModel,List< Vector2Int>> CellContentMovedByRoute;
    public Action<UnitModel> TurnStarted;
    public Action<List<Vector2Int>> routeChanged;
    [Inject] private UnitModelFactory _unitModelFactory;
    private GridXZ<GameCell> _grid;
    private MovementSystem _movementSystem;
    //private ActionService _actionService = new();
    public IAction CurrentAction = null;
    [Inject] private TurnSystem _turnSystem;
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
        UnitModel unit = _unitModelFactory.Create(unitSpawnParams);
        unit.TurnStarted += () => UnitModel_OnTurnStarted(unit); 

        var cell = (GameCell)GetCell(unitSpawnParams.X, unitSpawnParams.Y);
        AddContent(cell,unit);

        return new OperationResult(true);
    }

    private void UnitModel_OnTurnStarted(UnitModel unit)
    {
        GameCell cell = (GameCell)GetCell(unit.X, unit.Y);
        Debug.Log("turn started in cell" + cell.x + ", " + cell.y);
        CurrentAction = new MoveUnitAcion(_movementSystem, cell,unit.UnitState.MoveSpeed, (route) => CellContentMovedByRoute?.Invoke(unit,route));
        //CurrentAction = new TeleportUnitAction(cell,(coords)=> CellContentMoved?.Invoke(unit, coords));
        TurnStarted?.Invoke(unit);
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
        UnitModel unitModel = cell.GetUnit();
        CellContentRemoved?.Invoke(new UnitModelRemovedParams(unitModel));
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
            CellContentRemoved?.Invoke(new(unit));
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
        UnitModelCreatedParams unitModelCreatedParams = new UnitModelCreatedParams(content as UnitModel);
        CellContentAdded?.Invoke(unitModelCreatedParams);
    }

    private void RemoveContent(GameCell gameGridCell, IGridContent content)
    {
        gameGridCell.RemoveContent(content);
        UnitModelRemovedParams unitModelRemovedParams = new UnitModelRemovedParams(content as UnitModel);
        CellContentRemoved?.Invoke(unitModelRemovedParams);
    }

    public bool ContainsUnit(Vector2Int coords)
    {
        return ((GameCell)GetCell(coords)).ContainsUnit();
    }

    public List<Vector2Int> GetAvailableMovePoints(Vector2Int from,int speed)
    {
       return _movementSystem.GetReachableCells(from,speed);
    }

    public List<Vector2Int> GetAvailableMovePoints(int x,int y,int speed)
    {
        return GetAvailableMovePoints(new Vector2Int(x,y),speed);
    }

    public IReadOnlyList<UnitModel> GetUnits()
    {
        return GetAllCells().Where(x=> x.Unit != null).Select(x=> x.Unit).ToList();
    }

    public void PerformAction(GameCell gameCell)
    {
        if (CurrentAction.Perform(gameCell))
        {
            _turnSystem.EndTurn();
        }
    }
}
