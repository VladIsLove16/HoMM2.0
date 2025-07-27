using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MoveThenAttackHandlerFactory : PlaceholderFactory<UnitModel, MoveThenAttackHandler>
{
    [Inject] private DiContainer _container;

    public override MoveThenAttackHandler Create(UnitModel unit)
    {
        // Создаём нужный подтип
        var handler = new MoveThenAttackHandler(unit);

        // Внедряем зависимости, помеченные [Inject]
        _container.Inject(handler);

        return handler;
    }
}

public class MoveThenAttackHandler : IActionHandler
{ 
    [Inject] private MovementSystem _movementSystem;
    [Inject] private GameModel _gameModel;
    [Inject] private VisualHintSystem _hints;
    [Inject] private IGridCellRenderer _renderer;

    private UnitModel _activeUnit;
    private List<Vector2Int> savedRoute = new();
    public MoveThenAttackHandler(UnitModel unitModel)
    {
        _activeUnit = unitModel;    
    }
    public bool CanHandle(ActionContext ctx)
    {
        if(ctx.TargetObject == null)
            return false;
        var route = GetMoveRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        float fullRouteCost = _movementSystem.GetRouteCost(route);
        float moveRouteCost = _movementSystem.GetRouteCost(moveRoute);
        if(! _movementSystem.HasLineOfSight(_activeUnit.Position.Value, ctx.TargetCell))
            return false;
        if (moveRouteCost + _activeUnit.ModifiedStats.AttackRange >= fullRouteCost)
            return true;
        return false;
    }
    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetObject != null;
    }
    public void Execute(ActionContext ctx)
    {
        var moveRoute = GetMoveRoute(ctx);
        _gameModel.MoveUnit(_activeUnit, moveRoute);
        _activeUnit.SendDamage(new(ctx.TargetObject));
    }
    public void ShowPreview(ActionContext ctx)
    {
        int moveSpeed = _activeUnit.ModifiedStats.MoveSpeed;
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);

        _renderer.RemoveStates(CellState.accessibleRoutePoint);
        _renderer.RemoveStates(CellState.inaccessibleRoutePoint);
        _renderer.AddStates(moveRoute, CellState.accessibleRoutePoint);
        _renderer.AddStates(moveRoute, CellState.inaccessibleRoutePoint);

        var movaAvailableCells = _movementSystem.GetReachableCells(_activeUnit.Position.Value, _activeUnit.ModifiedStats.MoveSpeed);
        _renderer.AddStates(movaAvailableCells, CellState.moveAvailable);

        DamageContext damageContext = _activeUnit.SimulateSendDamage(new(ctx.TargetObject));
        _hints.ShowAttackHint(ctx.TargetCell, damageContext, CursorState.Attack);
    }
    private List<Vector2Int> GetRoute(ActionContext ctx)
    {
        var route = new List<Vector2Int>();
        if (_activeUnit.ModifiedStats.CanFly)
        {
            _movementSystem.GetRouteIgnoringObstacles(_activeUnit.Position.Value, ctx.TargetCell, out route);
        }
        else
            _movementSystem.GetRoute(_activeUnit.Position.Value, ctx.TargetCell, out route);
        return route;
    }
    private static List<Vector2Int> GetInaccessibleRoute(List<Vector2Int> route, List<Vector2Int> moveRoute)
    {
        var inaccessRoute = route.ToList();
        foreach (var movePoint in moveRoute)
        {
            inaccessRoute.Remove(movePoint);
        }

        return inaccessRoute;
    }
    private List<Vector2Int> GetMoveRoute(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        int moveSpeed = _activeUnit.ModifiedStats.MoveSpeed;
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(route, moveSpeed);
        return moveRoute;
    }

    public void ShowAvaiableTargetCells()
    {
        var units = _gameModel.GetUnits();
        List<Vector2Int> unitPositions = units.Select(x=>x.Position.Value).ToList();
        foreach(var unit in units)
        {
            ActionContext ctx = new ActionContext() { TargetCell = unit.Position.Value, TargetObject = unit,AbilityUsed = null }; 
            if(!CanHandle(ctx))
            {
                unitPositions.Remove(unit.Position.Value);
            }
        }
        _renderer.RemoveStates(CellState.moveAvailable);
        _renderer.AddStates(unitPositions,CellState.moveAvailable);
    }
}
