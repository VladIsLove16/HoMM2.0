using NUnit.Framework;
using System;
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
    [Inject] protected ITurnService TurnService;

    public MoveActionHandler(MovementSystem movementSystem, GameModel model)
    {
        _gameModel = model;
        _movementSystem = movementSystem;
    }
    public void Execute(ActionContext ctx)
    {
        if(!CanExecute(ctx))
            throw new InvalidOperationException("Cant execute MoveActionHandler " + ctx.ToString());
        IMoveable moveable = _gameModel.GetCell(ctx.FromCell).Unit as IMoveable;

        bool accessibleExist = GetAccessibleRoute(moveable, ctx.TargetCell, out var accessiblemoveRoute);
        if (_movementSystem.GetRouteCost(accessiblemoveRoute) > moveable.MoveSpeed)
        {
            Debug.LogError("Cant execute MoveActionHandler " + ctx.ToString());
            return;
        }
        if (moveable == null)
        {
            Debug.LogError("Cant execute MoveActionHandler " + ctx.ToString());
            return;
        }
        Debug.Log("Execute moveHandler ");
        _gameModel.MoveObject(moveable, accessiblemoveRoute);
    }
    public bool CanExecute(ActionContext ctx)
    {
        IMoveable moveable = _gameModel.GetCell(ctx.FromCell).Unit as IMoveable;
        if (moveable == null)
            return false;
        bool accessibleExist = GetAccessibleRoute(moveable, ctx.TargetCell, out var accessiblemoveRoute);
        if(!accessibleExist)    
            return false;
        if (_movementSystem.GetRouteCost(accessiblemoveRoute) > moveable.MoveSpeed)
        {
            return false;
        }
        return true;
    }
    public PreviewResult GetPreview(ActionContext ctx)
    {
        var result = new PreviewResult();
        var moveable = _gameModel.GetCell(ctx.FromCell).Unit as IMoveable;

        var routeExist = GetRoute(moveable, ctx.TargetCell,out var route);
        if (!routeExist)
            return new();
        var AccessiblrouteExist = GetAccessibleRoute(moveable, ctx.TargetCell,out var accessible);
        var inaccessible = GetInaccessibleRoute(route,accessible);

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

    private bool GetRoute(IMoveable moveable, Vector2Int targetCell, out List< Vector2Int> route)
    {
        bool result;
        if (moveable.CanFly)
        {
            result = _movementSystem.GetRouteIgnoringObstacles(moveable.Position, targetCell, out route);
        }
        else
            result = _movementSystem.GetRoute(moveable.Position, targetCell, out route);
        return result;
    }
    private bool GetAccessibleRoute(IMoveable moveable, Vector2Int targetCell, out List<Vector2Int> route)
    {
        var result = GetRoute(moveable, targetCell,out route);
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(route, moveable.MoveSpeed);
        return result;
    }
}
