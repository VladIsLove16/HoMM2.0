using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using Vector2Int = UnityEngine.Vector2Int;
using Debug = UnityEngine.Debug;

public class GameModel
{
    public Action<GridXZ<GameCell>> GameChange_Initialized;
    public Action<UnitModelCreatedParams> GameChange_UnitSpawned;
    public Action<List<Vector2Int>> GameChange_UnitMovedByRoute;

    private readonly UnitModelFactory _unitFactory;
    private readonly MovementSystem _movement;
    protected GridXZ<GameCell> _grid;
    private MovementSystem movementSystem;

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
        GameChange_Initialized?.Invoke(_grid);
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

        //unit.Died += () => UnitDied?.Invoke(new ContentDiedParams(unit));
        var unitModelCreatedParams = new UnitModelCreatedParams(unit);
        GameChange_UnitSpawned?.Invoke(unitModelCreatedParams);
        Debug.Log("unit spawned " + new Vector2Int(spawnParams.X, spawnParams.Y));
        return new(true);
    }

    public virtual void MoveObject(IMoveable gridContent, List<Vector2Int> path)
    {
        Debug.Log("MoveObject " + path.Count);
        var endCellCoords = path[path.Count - 1];
        var endCell = _grid.GetGridObject(endCellCoords.x, endCellCoords.y);
        endCell.AddContent(gridContent);
        var currentCell = _grid.GetGridObject(gridContent.Position.x, gridContent.Position.y);
        currentCell.RemoveContent(gridContent);
        gridContent.MoveByRoute(path);
        GameChange_UnitMovedByRoute?.Invoke(path);
    }

    public virtual void MoveObject(IGridContent gridContent, Vector2Int coords)
    {
        var endCell = _grid.GetGridObject(coords.x, coords.y);
        endCell.AddContent(gridContent);
        var currentCell = _grid.GetGridObject(gridContent.Position.x, gridContent.Position.y);
        currentCell.RemoveContent(gridContent);
        gridContent.Position = coords;

    }

    public (int, int) GetRandomEmpty()
    {
        foreach (var cell in GetAllCells())
            if (cell.IsEmpty) return (cell.X, cell.Y);
        return (0, 0); // fallback
    }

    public IGridCell GetCell(Vector2Int pos)
    {
        if(!_grid.IsInBounds(pos.x,pos.y))
        {
            return new GameCell(null,0,0);
        }
        return _grid.GetGridObject(pos.x, pos.y);
    }
   
    public void ClearGrid()
    {
        for(int i = 0; i < _grid.GetHeight(); i++)
            for (int j = 0; j < _grid.GetWidth(); j++)
            {
                var cell = _grid.GetGridObject(i, j);
                cell.Clear();
            }
    }

    public List<UnitModel> GetUnits()
    {
        var units = new List<UnitModel>();
        foreach (var cellInterface in GetAllCells())
        {
            var cell = cellInterface as GameCell;
            if (cell == null) continue;
            if (cell.Unit is UnitModel unit)
                units.Add(unit);
        }
        return units;
    }
    protected virtual GameCell CreateEmptyGameGridObject(GridXZ<GameCell> grid, int x, int y)
    {
        return new GameCell(grid, x, y);
    }
}
