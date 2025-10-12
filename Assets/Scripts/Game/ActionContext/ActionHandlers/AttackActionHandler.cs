using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackActionHandler : IActionHandler, IAttackActionHandler
{
    public ActionType ActionType
    {
        get
        {
            return ActionType.Attack;
        }
    }
    MovementSystem _movementSystem;
    GameModel _gameModel;
    public AttackActionHandler(MovementSystem movementSystem, GameModel gameModel)
    {
        _movementSystem = movementSystem;
        _gameModel = gameModel;
    }
    public void Execute(ActionContext ctx)
    {
        var fromCell = _gameModel.GetCell(ctx.FromCell);
        var toCell = _gameModel.GetCell(ctx.TargetCell);
        var attacker = fromCell?.Unit as IDamageSource;
        var target = toCell?.Unit as IDamagable;

        if (attacker == null || target == null)
        {
            Debug.LogWarning("AttackActionHandler.Execute called with invalid attacker or target. Aborting.");
            return;
        }

        attacker.SendDamage(new(target, true)); 
    }
    public bool CanExecute(ActionContext ctx)
    {
        var fromCell = _gameModel.GetCell(ctx.FromCell);
        if (fromCell == null || fromCell.Unit == null)
            return false;

        var attacker = fromCell.Unit;

        var toCell = _gameModel.GetCell(ctx.TargetCell);
        if (toCell == null || toCell.Unit == null)
            return false;

        var target = toCell.Unit as IDamagable;
        if (target == null)
            return false;

        if(target.Team == attacker.Team.Value)
            return false;

        _movementSystem.GetRoute(ctx.FromCell, ctx.TargetCell, out var route);
        if (route == null)
            return false;

        var routeCost = _movementSystem.GetRouteCost(route);

        if (attacker.ModifiedStats == null)
            return false;

        if (Mathf.Floor(routeCost)> attacker.ModifiedStats.AttackRange)
            return false;
        return true;
    }

    public PreviewResult GetPreview(ActionContext ctx)
    {
        PreviewResult previewResult = new();
        previewResult.Add(CellState.attackTarget, new List<Vector2Int>(){ ctx.TargetCell });
        return previewResult;
    }

    public DamageContextPreview GetDamagePreview(ActionContext ctx)
    {
        IDamagable damagable = _gameModel.GetCell(ctx.TargetCell).Unit;
        IDamageSource damageSource = _gameModel.GetCell(ctx.FromCell).Unit;
        AttackContext attackContext = new(damagable, false);
        DamageContext damageContext = damageSource.SimulateSendDamage(attackContext);
        return new(damageContext);
    }
}