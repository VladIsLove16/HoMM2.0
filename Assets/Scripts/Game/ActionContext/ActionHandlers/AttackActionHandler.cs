using NUnit.Framework;
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
        var attacker = _gameModel.GetCell(ctx.FromCell).Unit as IDamageSource;
        var target = _gameModel.GetCell(ctx.TargetCell).Unit as IDamagable;
        attacker.SendDamage(new(target, true));
    }
    public bool CanExecute(ActionContext ctx)
    {
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