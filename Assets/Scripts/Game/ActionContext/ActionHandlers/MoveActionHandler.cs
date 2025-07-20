using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Zenject;

public class MoveActionHandler : IActionHandler
{
    [Inject] private MovementSystem _movementSystem;
    [Inject] private VisualHintSystem _hints;
    private List<Vector2Int> lastSavedRoute = new();
    private Vector2Int lastSavedpoint;
    public bool CanHandle(ActionContext ctx)
    {
        return ctx.TargetUnit == null;
    }
    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetUnit == null;
    }

    public IEnumerator Execute(ActionContext ctx)
    {
        List<Vector2Int> moveRoute = GetMoveRoute(ctx);
        yield return ctx.Unit.MoveAlongRoute(moveRoute);
    }

    public void ShowPreview(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);
        _hints.ShowRoute(moveRoute, inaccessRoute);
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
        if (ctx.Unit.Model.ModifiedStats.CanFly)
        {
            _movementSystem.GetRouteIgnoringObstacles(ctx.Unit.Model.Position.Value, ctx.TargetCell, out route);
        }
        else
            _movementSystem.GetRoute(ctx.Unit.Model.Position.Value, ctx.TargetCell, out route);
        return route;
    }
    private List<Vector2Int> GetMoveRoute(ActionContext ctx)
    {
        var route = GetRoute(ctx);
        int moveSpeed = ctx.Unit.Model.ModifiedStats.MoveSpeed;
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(route, moveSpeed);
        return moveRoute;
    }

}
