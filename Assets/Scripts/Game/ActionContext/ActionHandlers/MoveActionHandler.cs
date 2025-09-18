using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
public class MoveActionHandlerFactory : PlaceholderFactory<ICombatObject, MoveActionHandler>
{
    [Inject] private DiContainer _container;

    public override MoveActionHandler Create(ICombatObject unit)
    {
        // Создаём нужный подтип
        MovementSystem movementSystem = _container.Resolve<MovementSystem>();
        movementSystem.HasLineOfSight(new Vector2Int(0,0), new Vector2Int(1,1));
        var handler = new MoveActionHandlerDebugger(unit, movementSystem);

        _container.Inject(handler);

        return handler;
    }
}

public class MoveActionHandlerDebugger : MoveActionHandler
{
    public MoveActionHandlerDebugger(ICombatObject model, MovementSystem movementSystem) : base(model, movementSystem) { }
}

public class MoveActionHandler : IActionHandler
{
    private ICombatObject _activeUnit;
    [Inject] protected MovementSystem _movementSystem;
    [Inject] protected GameModel _gm;
    private List<Vector2Int> reachableCells;
    public MoveActionHandler(ICombatObject model, MovementSystem movementSystem)
    {
        _activeUnit = model;
        _movementSystem = movementSystem;
        reachableCells = _movementSystem.GetReachableCells(model.Position, model.Stats.MoveSpeed);
    }
    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetObject == null;
    }

    public void Execute(ActionContext ctx)
    {
        // ActionHandler теперь только определяет логику, выполнение через GameModel
        List<Vector2Int> moveRoute = GetMoveRoute(ctx);
        if (moveRoute == null || moveRoute.Count == 0)
            return;

        // Выполняем движение через GameModel
        if (_activeUnit is IMoveable moveable)
        {
            _gm.MoveObject(moveable, moveRoute);
        }
    }

    public ActionPreview GetPreview(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);
        
        return new ActionPreview
        {
            MoveRoute = moveRoute,
            InaccessibleRoute = inaccessRoute,
            ReachableCells = reachableCells,
            IsActionAvailable = inaccessRoute.Count == 0,
            Damage = null
        };
    }
    public List<Vector2Int> GetAvailableTargetCells()
    {
        return reachableCells;
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
    private List<Vector2Int> GetMoveRoute(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        int moveSpeed = _activeUnit.Stats.MoveSpeed;
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(route, moveSpeed);
        return moveRoute;
    }
}
