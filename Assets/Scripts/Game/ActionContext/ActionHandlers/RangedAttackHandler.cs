using System.Collections;
using UnityEngine;
using Zenject;

internal class RangedAttackHandler : IActionHandler
{
    [Inject] MovementSystem _movementSystem;
    [Inject] GameModel _gameModel;
    [Inject] VisualHintSystem _hints;
    public bool CanHandle(ActionContext ctx)
    {
        //_movementSystem.GetRouteIgnoringObstacles(ctx.Unit.Model.Position.Value, ctx.TargetCell, out var attackRoute);
        //var routeCost =_movementSystem.GetRouteCost(attackRoute);
        return _gameModel.IsInAttackRange(ctx.Unit.Model.Position.Value,ctx.TargetCell,ctx.Unit.Model.ModifiedStats.AttackRange);
    }
    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetUnit !=null;
    }

    public IEnumerator Execute(ActionContext ctx)
    {
        yield return ctx.Unit.Attack(ctx.TargetUnit);
    }

    public void ShowPreview(ActionContext ctx)
    {
        int predictedDamage = ctx.Unit.Model.ModifiedStats.Damage;

        _hints.ShowAttackHint(ctx.TargetCell, predictedDamage, icon, "Ranged attack");
    }
}