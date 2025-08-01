using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MoveThenAttackHandlerFactory : PlaceholderFactory<ICombatObject, MoveThenAttackHandler>
{
    [Inject] private DiContainer _container;

    public override MoveThenAttackHandler Create(ICombatObject unit)
    {
        var handler = new MoveThenAttackHandler(unit);

        _container.Inject(handler);

        return handler;
    }
}

public class MoveThenAttackHandler : IActionHandler
{ 
    [Inject] private MovementSystem _movementSystem;
    [Inject] private GameModel _gameModel;
    [Inject] private IGridCellRenderer _renderer;
    [Inject] private IAttackActionPanel _attackPanel;

    private ICombatObject _activeUnit;
    private List<Vector2Int> savedRoute = new();
    public MoveThenAttackHandler(ICombatObject unitModel)
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
        if(! _movementSystem.HasLineOfSight(_activeUnit.Position, ctx.TargetCell))
            return false;
        if (moveRouteCost + _activeUnit.Stats.AttackRange >= fullRouteCost)
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
        if(_activeUnit is IMoveable moveable)
        {
            _gameModel.MoveObject(moveable, moveRoute);
        }
        if (_activeUnit is IDamageSource source)
            source.SendDamage(new(ctx.TargetObject));
    }
    public void ShowPreview(ActionContext ctx)
    {
        int moveSpeed = _activeUnit. Stats.MoveSpeed;
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);

        _renderer.RemoveStates(CellState.accessibleRoutePoint);
        _renderer.RemoveStates(CellState.inaccessibleRoutePoint);
        _renderer.AddStates(moveRoute, CellState.accessibleRoutePoint);
        _renderer.AddStates(moveRoute, CellState.inaccessibleRoutePoint);

        var movaAvailableCells = _movementSystem.GetReachableCells(_activeUnit.Position, _activeUnit.Stats.MoveSpeed);
        _renderer.AddStates(movaAvailableCells, CellState.moveAvailable);

        if(_activeUnit is IDamageSource source)
        {
            DamageContext damageContext = source.SimulateSendDamage(new(ctx.TargetObject));
            var info = new AttackPreviewInfo
            {
                TargetPosition = ctx.TargetCell,
                DamageContext = damageContext,
                Description = damageContext.DamageAmount.ToString(),
            };
            _attackPanel.Show(info);
        }
    }
    private List<Vector2Int> GetRoute(ActionContext ctx)
    {
        var route = new List<Vector2Int>();
        if (_activeUnit.Stats.CanFly)
        {
            _movementSystem.GetRouteIgnoringObstacles(_activeUnit.Position, ctx.TargetCell, out route);
        }
        else
            _movementSystem.GetRoute(_activeUnit.Position, ctx.TargetCell, out route);
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
        int moveSpeed = _activeUnit.Stats.MoveSpeed;
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
        _renderer.AddStates(unitPositions,CellState.moveAvailable);
    }

    public void Cancel()
    {
        
    }

    public void HidePreview()
    {
        _attackPanel.Hide();
    }
}
