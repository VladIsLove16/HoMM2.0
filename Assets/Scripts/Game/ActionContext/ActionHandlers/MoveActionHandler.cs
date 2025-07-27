using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
public class MoveActionHandlerFactory : PlaceholderFactory<UnitModel, MoveActionHandler>
{
    [Inject] private DiContainer _container;

    public override MoveActionHandler Create(UnitModel unit)
    {
        // Создаём нужный подтип
        var handler = new MoveActionHandlerDebugger(unit);

        _container.Inject(handler);

        return handler;
    }
}


public class MoveActionHandlerDebugger : MoveActionHandler
{
    public MoveActionHandlerDebugger(UnitModel model) : base(model) { }

    public override bool CanHandle(ActionContext ctx)
    {
        bool res = base.CanHandle(ctx);
        if (!res)
        {
            Debug.Log("MoveActionHandlerDebugger: can't handle action at " + ctx.TargetCell);
        }
        return res;
    }
}


public class MoveActionHandler : IActionHandler
{
    private UnitModel _activeUnit;
    [Inject] protected MovementSystem _movementSystem;
    [Inject] protected IGridCellRenderer _renderer;
    [Inject] protected VisualHintSystem _hints;
    [Inject] protected GameModel _gm;
    private List<Vector2Int> lastSavedRoute = new();
    private Vector2Int lastSavedpoint;
    public MoveActionHandler(UnitModel model)
    {
        _activeUnit = model;
    }
    public virtual bool CanHandle(ActionContext ctx)
    {
        return ctx.TargetObject == null;
    }
    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetObject == null;
    }

    public void Execute(ActionContext ctx)
    {
        List<Vector2Int> moveRoute = GetMoveRoute(ctx);
        _gm.MoveUnit(_activeUnit, moveRoute);
    }

    public void ShowPreview(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);
        _renderer.RemoveStates(CellState.accessibleRoutePoint);
        _renderer.RemoveStates(CellState.inaccessibleRoutePoint);
        _renderer.RemoveStates(CellState.moveAvailable);
        _renderer.AddStates(moveRoute, CellState.accessibleRoutePoint);
        _renderer.AddStates(inaccessRoute, CellState.inaccessibleRoutePoint);
    }

    public void ShowAvaiableTargetCells()
    {
        var pos = _activeUnit.Position.Value;
        var stats = _activeUnit.ModifiedStats;
        var speed = stats.MoveSpeed;
        _renderer.AddState(new(0, 0), CellState.moveAvailable);
        var movaAvailableCells = _movementSystem.GetReachableCells(pos,speed);
        _renderer.RemoveStates(CellState.moveAvailable);
        _renderer.AddStates(movaAvailableCells, CellState.moveAvailable);
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
        if (_activeUnit.ModifiedStats.CanFly)
        {
            _movementSystem.GetRouteIgnoringObstacles(_activeUnit.Position.Value, ctx.TargetCell, out route);
        }
        else
            _movementSystem.GetRoute(_activeUnit.Position.Value, ctx.TargetCell, out route);
        return route;
    }
    private List<Vector2Int> GetMoveRoute(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        int moveSpeed = _activeUnit.ModifiedStats.MoveSpeed;
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(route, moveSpeed);
        return moveRoute;
    }

}
