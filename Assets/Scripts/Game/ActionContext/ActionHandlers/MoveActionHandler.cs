using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class MoveActionHandler : IActionHandler
{
    public ActionType ActionType
    {
        get
        {
            return ActionType.Move;
        }
    }

    protected MovementSystem _movementSystem;
    protected GameModel _gameModel;

    public MoveActionHandler(MovementSystem movementSystem, GameModel model)
    {
        _gameModel = model;
        _movementSystem = movementSystem;
    }
    public void Execute(ActionContext ctx)
    {
        IMoveable moveable = _gameModel.GetCell(ctx.FromCell).Unit as IMoveable;

        List<Vector2Int> moveRoute = GetAccessibleRoute(moveable, ctx.TargetCell);
        if (_movementSystem.GetRouteCost(moveRoute) > moveable.MoveSpeed)
        {
            Debug.LogError("Cant execute MoveActionHandler " + ctx.ToString());
            return;
        }
        if (moveable == null)
        {
            Debug.LogError("Cant execute MoveActionHandler " + ctx.ToString());
            return;
        }
        _gameModel.MoveObject(moveable, moveRoute);
    }
    public bool CanExecute(ActionContext ctx)
    {
        IMoveable moveable = _gameModel.GetCell(ctx.FromCell).Unit as IMoveable;
        if (moveable == null)
            return false;
        List<Vector2Int> moveRoute = GetAccessibleRoute(moveable, ctx.TargetCell);
        if (_movementSystem.GetRouteCost(moveRoute) > moveable.MoveSpeed)
        {
            return false;
        }
        return true;
    }
    public PreviewResult GetPreview(ActionContext ctx)
    {
        var result = new PreviewResult();
        var moveable = _gameModel.GetCell(ctx.FromCell).Unit as IMoveable;

        var route = GetRoute(moveable, ctx.TargetCell);
        var accessible = GetAccessibleRoute(moveable, ctx.TargetCell);
        var inaccessible =GetInaccessibleRoute(route,accessible);

        result.Add(CellState.hovered, new[] { ctx.TargetCell });
        result.Add(CellState.accessibleRoutePoint, accessible);
        result.Add(CellState.inaccessibleRoutePoint, inaccessible);
        return result;
    }

    private List<Vector2Int> GetInaccessibleRoute(List<Vector2Int> route, List<Vector2Int> accesibleRoute)
    {
        var inaccessRoute = route.ToList();
        foreach (var movePoint in accesibleRoute)
        {
            inaccessRoute.Remove(movePoint);
        }

        return inaccessRoute;
    }

    private List<Vector2Int> GetRoute(IMoveable moveable, Vector2Int targetCell)
    {
        var route = new List<Vector2Int>();
        if (moveable.CanFly)
        {
            _movementSystem.GetRouteIgnoringObstacles(moveable.Position, targetCell, out route);
        }
        else
            _movementSystem.GetRoute(moveable.Position, targetCell, out route);
        return route;
    }
    private List<Vector2Int> GetAccessibleRoute(IMoveable moveable, Vector2Int targetCell)
    {
        var route = GetRoute(moveable, targetCell);
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(route, moveable.MoveSpeed);
        return moveRoute;
    }
}
