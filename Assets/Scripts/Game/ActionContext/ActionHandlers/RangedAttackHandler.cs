using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
public class RangedAttackHandlerFactory : PlaceholderFactory<UnitModel, RangedAttackHandler>
{
    [Inject] private DiContainer _container;

    public override RangedAttackHandler Create(UnitModel unit)
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
    [Inject] VisualHintSystem _hints;
    [Inject] IGridCellRenderer _renderer;
    public UnitModel _activeUnit;
    public RangedAttackHandler(UnitModel unitModel)
    {
        _activeUnit = unitModel;
    }
    public bool CanHandle(ActionContext ctx)
    {
        var from = _activeUnit.Position.Value;
        var to = ctx.TargetCell;

        return _movementSystem.HasLineOfSight(from, to);
    }


    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetObject !=null;
    }

    public void Execute(ActionContext ctx)
    {
        _activeUnit.SendDamage(new(ctx.TargetObject));
    }

    public void ShowPreview(ActionContext ctx)
    {
        if (!CanHandle(ctx))
            return;
        DamageContext damageContext = _activeUnit.SimulateSendDamage(new(_activeUnit));
        _hints.ShowAttackHint(ctx.TargetCell, damageContext, CursorState.RangedAttack, "Ranged attack");
        var movaAvailableCells = _movementSystem.GetReachableCells(_activeUnit.Position.Value, _activeUnit.ModifiedStats.MoveSpeed);
        _renderer.AddStates(movaAvailableCells, CellState.moveAvailable);
    }

    public void ShowAvaiableTargetCells()
    {
        var units = _gameModel.GetUnits();
        List<Vector2Int> unitPositions = units.Select(x => x.Position.Value).ToList();
        foreach (var unit in units)
        {
            ActionContext ctx = new ActionContext() { TargetCell = unit.Position.Value, TargetObject = unit, AbilityUsed = null };
            if (!CanHandle(ctx))
            {
                unitPositions.Remove(unit.Position.Value);
            }
        }
        _renderer.RemoveStates(CellState.moveAvailable);
        _renderer.AddStates(unitPositions, CellState.moveAvailable);
    }
}