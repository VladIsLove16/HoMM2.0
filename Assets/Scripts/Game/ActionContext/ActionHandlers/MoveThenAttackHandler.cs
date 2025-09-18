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

    private ICombatObject _activeUnit;
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
        // ActionHandler теперь только определяет логику, выполнение через GameModel
        var moveRoute = GetMoveRoute(ctx);
        if(_activeUnit is IMoveable moveable)
        {
            _gameModel.MoveObject(moveable, moveRoute);
        }
        if (_activeUnit is IDamageSource source)
            source.SendDamage(new(ctx.TargetObject));
    }
    public ActionPreview GetPreview(ActionContext ctx)
    {
        int moveSpeed = _activeUnit.Stats.MoveSpeed;
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);
        var reachableCells = _movementSystem.GetReachableCells(_activeUnit.Position, _activeUnit.Stats.MoveSpeed);

        DamageContext damage = null;
        if(_activeUnit is IDamageSource source)
        {
            damage = source.SimulateSendDamage(new(ctx.TargetObject));
        }

        return new ActionPreview
        {
            MoveRoute = moveRoute,
            InaccessibleRoute = inaccessRoute,
            ReachableCells = reachableCells,
            IsActionAvailable = CanHandle(ctx),
            Damage = damage
        };
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

    //public List<Vector2Int> GetAvailableTargetCells()
    //{
    //    var units = _gameModel.GetUnits();
    //    var availableTargets = new List<Vector2Int>();
        
    //    foreach (var unit in units)
    //    {
    //        ActionContext ctx = new ActionContext { TargetCell = unit.Position, TargetObject = unit, AbilityUsed = null };
    //        if (CanHandle(ctx))
    //        {
    //            availableTargets.Add(unit.Position);
    //        }
    //    }
        
    //    return availableTargets;
    //}
}

