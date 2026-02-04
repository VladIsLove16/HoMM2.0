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
        var unit = _gameModel.GetCell(ctx.FromCell).Unit;
        if(unit == null)
            return false;
        var target = _gameModel.GetCell(ctx.TargetCell).Unit;
        if(target == null)
            return false;
        if (unit.Team == target.Team)
            return false;
        var dist = GetDistance(ctx.FromCell, ctx.TargetCell);
        bool isAdjacent = dist <= 1.01f;
        if (unit.ModifiedStats == null)
            return false;
        if (!unit.ModifiedStats.AllowAdjacentRanged && isAdjacent)
            return false;
        return dist <= unit.ModifiedStats.AttackRange;
    }

    private float GetDistance(Vector2Int from, Vector2Int to)
    {
        _movementSystem.GetRouteIgnoringObstacles(from, to, out var route);
        return _movementSystem.GetRouteCost(route);
    }
    public PreviewResult GetPreview(ActionContext actionContext)
    {
        PreviewResult previewResult = new();
        previewResult.Add(CellState.attackTarget, new List<Vector2Int>() { actionContext.TargetCell });
        return previewResult;
    }

    public DamageContext GetDamageContext(ActionContext ctx)
    {
        IDamagable damagable = _gameModel.GetCell(ctx.TargetCell).Unit;
        IDamageSource damageSource = _gameModel.GetCell(ctx.FromCell).Unit;
        AttackContext attackContext = new(damagable, true);
        DamageContext damageContext = damageSource.SimulateSendDamage(attackContext);
        return damageContext;
    }
}
