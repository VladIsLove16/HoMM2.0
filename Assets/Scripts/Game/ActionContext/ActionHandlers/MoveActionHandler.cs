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
    [Inject] protected IGridCellRenderer _renderer;
    [Inject] protected GameModel _gm;
    [Inject] IAttackActionPanel _attackPanel;
    [Inject] ICursorService CursorService;
    [Inject] private NetworkUnitCommandService _commandService;
    private List<Vector2Int> lastSavedRoute = new();
    private Vector2Int lastSavedpoint;
    private List<Vector2Int> reachableCells;
    private Dictionary<CellState, List<Vector2Int>> _preview = new();
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
        List<Vector2Int> moveRoute = GetMoveRoute(ctx);
        if (moveRoute == null || moveRoute.Count == 0)
            return;

        // Отправляем команду через сервис приложений (сетевой/локальный)
        if (_activeUnit is UnitModel unitModel)
        {
            _commandService.SendMove(unitModel, moveRoute);
            return;
        }

        // Fallback: если тип не UnitModel, но можно двигать как IMoveable (редкий случай)
        if (_activeUnit is IMoveable moveable)
        {
            _gm.MoveObject(moveable, moveRoute);
        }
    }

    public void ShowPreview(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);
        AddPreview(moveRoute, CellState.accessibleRoutePoint);
        AddPreview(inaccessRoute, CellState.inaccessibleRoutePoint);
        AddPreview(reachableCells, CellState.moveAvailable);
        if(inaccessRoute.Count == 0)
            CursorService.SetCursorState(CursorState.ActionAvailable);
        else
            CursorService.SetCursorState(CursorState.ActionNotAvailable);
    }

    private void AddPreview(List<Vector2Int> cells, CellState state)
    {
        _preview[state] = cells.ToList();
        _renderer.SetStates(cells, state);
    }

    public void HidePreview()
    {
        foreach (var state in _preview)
        {
            _renderer.RemoveStates(state.Key);
        }
        _preview.Clear();
        _attackPanel.Hide();
    }
    public void ShowAvaiableTargetCells()
    {
        var pos = _activeUnit.Position;
        var stats = _activeUnit.Stats;
        var speed = stats.MoveSpeed;
        var movaAvailableCells = _movementSystem.GetReachableCells(pos,speed);
        AddPreview(movaAvailableCells, CellState.moveAvailable);
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
