using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
public class RangedAttackHandler : IActionHandler
{
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
        var attacker = _gameModel.GetCell(ctx.FromCell).Unit as IDamageSource;
        var target = _gameModel.GetCell(ctx.TargetCell).Unit as IDamagable;
        attacker.SendDamage(new(target, true));
    }
    public bool CanExecute(ActionContext ctx)
    {
       return true;
    }
    private bool CanShoot(Vector2Int from, Vector2Int to, int range)
    {
        _movementSystem.GetRouteIgnoringObstacles(from, to, out var route);
        var dist = _movementSystem.GetRouteCost(route);
        return dist <= range && _movementSystem.HasLineOfSight(from, to);
    }

    //public ActionPreview GetPreview(ActionContext ctx)
    //{
    //    DamageContext damage = null;
    //    if (_activeUnit is IDamageSource source)
    //    {
    //        damage = source.SimulateSendDamage(new(ctx.TargetObject));
    //    }
    //    var reachable = _movementSystem.GetReachableCells(_activeUnit.Position, _activeUnit.Stats.MoveSpeed);
    //    return new ActionPreview
    //    {
    //        MoveRoute = new(){ _activeUnit.Position },
    //        InaccessibleRoute = new(),
    //        ReachableCells = reachable,
    //        IsActionAvailable = !IsSameTeam(ctx.TargetObject),
    //        Damage = damage
    //    };
    //}

    //public List<Vector2Int> GetAvailableTargetCells()
    //{
    //    var units = _gameModel.GetUnits();
    //    var availableTargets = new List<Vector2Int>();

    //    foreach (var unit in units)
    //    {
    //        if (!IsSameTeam(unit) && IsInRange(new ActionContext { TargetCell = unit.Position, TargetObject = unit }))
    //        {
    //            availableTargets.Add(unit.Position);
    //        }
    //    }

    //    return availableTargets;
    //}

}