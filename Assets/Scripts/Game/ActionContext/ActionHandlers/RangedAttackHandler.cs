using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class RangedAttackHandlerFactory : PlaceholderFactory<ICombatObject, RangedAttackHandler>
{
    [Inject] private DiContainer _container;
    public override RangedAttackHandler Create(ICombatObject unit)
    {
        // Создаём нужный подтип
        var handler = new RangedAttackHandler(unit);

        // Внедряем зависимости, помеченные [Inject]
        _container.Inject(handler);

        return handler;
    }
}

public class RangedAttackHandler : IActionHandler
{
    [Inject] MovementSystem _movementSystem;
    [Inject] GameModel _gameModel;
    private Dictionary<CellState, List<Vector2Int>> _preview = new();
    public ICombatObject _activeUnit;
    public RangedAttackHandler(ICombatObject unitModel)
    {
        _activeUnit = unitModel;
    }
    private bool CanShoot(Vector2Int from, Vector2Int to, int range)
    {
        _movementSystem.GetRouteIgnoringObstacles(from, to, out var route);
        var dist = _movementSystem.GetRouteCost(route);
        return dist <= range && _movementSystem.HasLineOfSight(from, to);
    }

    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetObject !=null;
    }

    public void Execute(ActionContext ctx)
    {
        if (_activeUnit is IDamageSource source && ctx.TargetObject is IDamagable target)
        {
            source.SendDamage(new(target, true));
        }
        else
            throw new UnityException("_activeUnit is IDamageSource source && ctx.TargetObject is IDamagable target not true");
    }

    internal bool CanExecute(ActionContext ctx)
    {
        throw new NotImplementedException();
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