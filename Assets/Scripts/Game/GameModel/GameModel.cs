using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using Vector2Int = UnityEngine.Vector2Int;
using Debug = UnityEngine.Debug;

public class GameModel
{
    public event Action<UnitModelCreatedParams> UnitSpawned;
    public event Action<ContentDiedParams> UnitDied;
    public event Action<IGridContent, List<Vector2Int>> UnitMovedByRoute;
    public event Action<GridXZ<GameCell>> GridInitialized;
    private readonly UnitModelFactory _unitFactory;
    private readonly MovementSystem _movement;
    private readonly ActionHandlerFactory _actionHandlerFactory;
    protected GridXZ<GameCell> _grid;
    private UnitModelFactory unitModelFactory;
    private MovementSystem movementSystem;
    private ActionHandlerFactory actionHandlerFactory;

    [Inject]
    public GameModel(UnitModelFactory factory, MovementSystem movement, ActionHandlerFactory actionHandlerFactory)
    {
        _movement = movement;
        _unitFactory = factory;
        _actionHandlerFactory = actionHandlerFactory;
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

        // Транслируем смерть модели в событие удаления
        unit.Died += () => UnitDied?.Invoke(new ContentDiedParams(unit));
            var unitModelCreatedParams = new UnitModelCreatedParams(unit);
        UnitSpawned?.Invoke(unitModelCreatedParams);
        Debug.Log("unit spawned " + new Vector2Int(spawnParams.X, spawnParams.Y));
        return new(true);
    }

    public virtual void MoveObject(IMoveable gridContent, List<Vector2Int> path)
    {
        var endCellCoords = path[path.Count - 1];
        var endCell = _grid.GetGridObject(endCellCoords.x, endCellCoords.y);
        endCell.AddContent(gridContent);
        var currentCell = _grid.GetGridObject(gridContent.Position.x, gridContent.Position.y);
        currentCell.RemoveContent(gridContent);
        gridContent.MoveByRoute(path);
        UnitMovedByRoute?.Invoke(gridContent, path);
    }

    public (int, int) GetRandomEmpty()
    {
        foreach (var cell in GetAllCells())
            if (cell.IsEmpty) return (cell.X, cell.Y);
        return (0, 0); // fallback
    }

    public GameCell GetCell(Vector2Int pos) => _grid.GetGridObject(pos.x, pos.y);

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

    // MVVM input handling - called by GameViewModel
    public void OnGameModelObjectSelected(Vector2Int cellCoords)
    {
        // Process cell selection in the domain model
        // This could trigger action resolution, validation, etc.
        var cell = GetCell(cellCoords);
        if (cell != null)
        {
            // Handle cell selection logic here
            // Could determine available actions, validate moves, etc.
        }
    }

    // Action execution methods - единственное место для выполнения действий
    public void ExecuteMoveAction(UnitModel unit, List<Vector2Int> moveRoute)
    {
        if (unit == null || moveRoute == null || moveRoute.Count == 0)
            return;

        // Выполняем движение через доменную логику
        MoveObject(unit, moveRoute);
    }

    public void ExecuteAttackAction(UnitModel attacker, Vector2Int targetCell)
    {
        if (attacker == null)
            return;

        var targetCellObj = GetCell(targetCell);
        if (targetCellObj?.Unit is IDamagable target)
        {
            // Выполняем атаку через доменную логику
            if (attacker is IDamageSource source)
            {
                source.SendDamage(new(target));
            }
        }
    }

    public void ExecuteMoveThenAttackAction(UnitModel unit, List<Vector2Int> moveRoute, Vector2Int targetCell)
    {
        if (unit == null)
            return;

        // Сначала движение
        if (moveRoute != null && moveRoute.Count > 0)
        {
            MoveObject(unit, moveRoute);
        }

        // Затем атака
        ExecuteAttackAction(unit, targetCell);
    }

    // ActionHandler factory methods
    public IActionHandler GetMoveActionHandler(UnitModel unit)
    {
        return _actionHandlerFactory.CreateMoveHandler(unit);
    }

    public IActionHandler GetRangedAttackHandler(UnitModel unit)
    {
        return _actionHandlerFactory.CreateRangedAttackHandler(unit);
    }

    public IActionHandler GetMoveThenAttackHandler(UnitModel unit)
    {
        return _actionHandlerFactory.CreateMoveThenAttackHandler(unit);
    }

}
public class GameModelDebugger : GameModel
{
    public GameModelDebugger(UnitModelFactory factory, MovementSystem movement, ActionHandlerFactory actionHandlerFactory) 
        : base(factory, movement, actionHandlerFactory)
    {
        Debug.Log("GameModel is ready");
    }

    protected override GameCell CreateEmptyGameGridObject(GridXZ<GameCell> grid, int x, int y)
    {
        //Debug.Log("Creating endCell in " + x + ":" + y);
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
    public override void MoveObject(IMoveable unit, List<Vector2Int> path)
    {
         Debug.Log("MoveObject callled " + unit + " " + path.ToString());
         base.MoveObject(unit, path);
    }
}