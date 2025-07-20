using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveThenAttackHandler : IActionHandler
{
    private MovementSystem _movementSystem;
    private GameModel _gameModel;
    private VisualHintSystem _hints;
    private List<Vector2Int> savedRoute = new();
    public bool CanHandle(ActionContext ctx)
    {
        if(ctx.TargetUnit == null)
            return false;
        var route = GetMoveRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        float fullRouteCost = _movementSystem.GetRouteCost(route);
        float moveRouteCost = _movementSystem.GetRouteCost(moveRoute);
        _gameModel.IsInAttackRange(moveRoute[moveRoute.Count - 1], ctx.TargetCell, ctx.Unit.Model.ModifiedStats.AttackRange);
        if (moveRouteCost + ctx.Unit.Model.ModifiedStats.AttackRange >= fullRouteCost)
            return true;
        return false;
    }
    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetUnit != null;
    }
    public IEnumerator Execute(ActionContext ctx)
    {
        var moveRoute = GetMoveRoute(ctx);
        yield return ctx.Unit.MoveAlongRoute(moveRoute);
        yield return ctx.Unit.Attack(ctx.TargetUnit);
    }
    public void ShowPreview(ActionContext ctx)
    {
        int moveSpeed = ctx.Unit.Model.ModifiedStats.MoveSpeed;
        var route = GetRoute(ctx);
        var moveRoute = GetMoveRoute(ctx);
        var inaccessRoute = GetInaccessibleRoute(route, moveRoute);
        _hints.ShowRoute(moveRoute, inaccessRoute);
        _hints.ShowAttackHint(ctx.TargetCell, ctx.Unit.Model.ModifiedStats.Damage);
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
        int moveSpeed = ctx.Unit.Model.ModifiedStats.MoveSpeed;
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(route, moveSpeed);
        return moveRoute;
    }

}
