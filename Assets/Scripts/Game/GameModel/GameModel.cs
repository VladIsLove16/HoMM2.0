// GameModel.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

public class GameModel
{
    public event Action<UnitModelCreatedParams> UnitSpawned;
    public event Action<UnitModelRemovedParams> UnitRemoved;
    public event Action<UnitModel, List<Vector2Int>> UnitMovedByRoute;
    public event Action<GridXZ<GameCell>> GridInitialized;
    private readonly UnitModelFactory _unitFactory;
    private readonly MovementSystem _movement;
    protected GridXZ<GameCell> _grid;


    [Inject]
    public GameModel(UnitModelFactory factory, MovementSystem movement)
    {
        _movement = movement;
        _unitFactory = factory;
    }
    public virtual void InitializeGrid(int width, int height)
    {
        _grid = new GridXZ<GameCell>(width, height, CreateEmptyGameGridObject);
        _movement.Init(_grid);
        GridInitialized?.Invoke(_grid);
    }
    protected virtual GameCell CreateEmptyGameGridObject(GridXZ<GameCell> grid, int x, int y)
    {
        return new GameCell(grid, x, y);
    }

    public IGridCell[,] GetAllCells()
    {
        return  _grid.GetGridArray();
    }
    public bool IsInBounds(Vector2Int pos) => _grid.IsInBounds(pos.x, pos.y);

    public virtual OperationResult SpawnUnit(UnitSpawnParams spawnParams)
    {
        if (!IsInBounds(new Vector2Int(spawnParams.X, spawnParams.Y)))
            return new(false, "Out of bounds");

        var cell = _grid.GetGridObject(spawnParams.X, spawnParams.Y);
        if (!cell.IsEmpty)
            return new(false, "Cell not empty");

        var unit = _unitFactory.Create(spawnParams);
        cell.AddContent(unit);

        unit.Died += ()=> UnitRemoved?.Invoke(new UnitModelRemovedParams(unit));
        UnitSpawned?.Invoke(new UnitModelCreatedParams(unit));
        return new(true);
    }

    public void MoveUnit(UnitModel unit, List<Vector2Int> path)
    {
        foreach (var pos in path)
        {
            var cell = _grid.GetGridObject(pos.x, pos.y);
            cell.AddContent(unit);
        }
        UnitMovedByRoute?.Invoke(unit, path);
    }

    public (int, int) GetRandomEmpty()
    {
        foreach (var cell in GetAllCells())
            if (cell.IsEmpty) return (cell.X, cell.Y);
        return (0, 0); // fallback
    }

    public GameCell GetCell(Vector2Int pos) => _grid.GetGridObject(pos.x, pos.y);

    internal void ClearGrid()
    {
        _grid.ClearGrid();
    }

    public List<UnitModel> GetUnits()
    {
        return GetAllCells().OfType<UnitModel>().ToList();
    }
}
public class GameModelDebugger : GameModel
{
    public GameModelDebugger(UnitModelFactory factory, MovementSystem movement) : base(factory, movement)
    {
        Debug.Log("GameModel is ready");
    }

    protected override GameCell CreateEmptyGameGridObject(GridXZ<GameCell> grid, int x, int y)
    {
        //Debug.Log("Creating cell in " + x + ":" + y);
        return base.CreateEmptyGameGridObject(grid, x, y);
    }
    public override void InitializeGrid(int width, int height)
    {
        Debug.Log("Game model InitializeGrid called " + width + " " + height);
        base.InitializeGrid(width, height);
    }
    public override OperationResult SpawnUnit(UnitSpawnParams spawnParams)
    {
        Debug.Log("model.SpawnUnit called" + spawnParams.UnitType);
        return base.SpawnUnit(spawnParams);
    }
}