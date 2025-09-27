using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
public class RangedAttackHandler : IActionHandler, IAttackActionHandler
{
    public ActionType ActionType
    {
        get
        {
            return ActionType.RangedAttack;
        }
    }
    MovementSystem _movementSystem;
    GameModel _gameModel;
    //private Dictionary<CellState, List<Vector2Int>> _preview = new();
    //public ICombatObject _activeUnit;
    public RangedAttackHandler(MovementSystem movementSystem, GameModel gameModel)
    {
        _movementSystem = movementSystem;
        _gameModel = gameModel;
    }
    public void Execute(ActionContext ctx)
    {
        if (!CanExecute(ctx))
        {
            throw new InvalidOperationException();
        }
        var attacker = _gameModel.GetCell(ctx.FromCell).Unit as IDamageSource;
        var target = _gameModel.GetCell(ctx.TargetCell).Unit as IDamagable;
        attacker.SendDamage(new(target, true));
    }
    public bool CanExecute(ActionContext ctx)
    {
        var unit = _gameModel.GetCell(ctx.AttackFromCell).Unit;
        if(unit == null)
            return false;
        var target = _gameModel.GetCell(ctx.TargetCell).Unit;
        if(target == null)
            return false;
        if (unit.Team == target.Team)
            return false;
        return CanShoot(ctx.AttackFromCell,ctx.TargetCell, unit.ModifiedStats.AttackRange);
    }
    private bool CanShoot(Vector2Int from, Vector2Int to, int range)
    {
        _movementSystem.GetRouteIgnoringObstacles(from, to, out var route);
        var dist = _movementSystem.GetRouteCost(route);
        return dist <= range;
            //&& _movementSystem.HasLineOfSight(from, to);
    }
    public PreviewResult GetPreview(ActionContext actionContext)
    {
        PreviewResult previewResult = new();
        previewResult.Add(CellState.attackTarget, new List<Vector2Int>() { actionContext.TargetCell });
        return previewResult;
    }

    public DamageContextPreview GetDamagePreview(ActionContext ctx)
    {
        IDamagable damagable = _gameModel.GetCell(ctx.TargetCell).Unit;
        IDamageSource damageSource = _gameModel.GetCell(ctx.FromCell).Unit;
        AttackContext attackContext = new(damagable, true);
        DamageContext damageContext = damageSource.SimulateSendDamage(attackContext);
        return new(damageContext);
    }
}