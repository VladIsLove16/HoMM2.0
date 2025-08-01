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
    [Inject] IAttackActionPanel _attackPanel;
    [Inject] IGridCellRenderer _renderer;
    [Inject] ICursorService _cursorService;
    private Dictionary<CellState, List<Vector2Int>> _preview = new();
    public ICombatObject _activeUnit;
    public RangedAttackHandler(ICombatObject unitModel)
    {
        _activeUnit = unitModel;
    }
    private bool IsInRange(ActionContext ctx)
    {
        var from = _activeUnit.Position;
        var to = ctx.TargetCell;

        return _movementSystem.HasLineOfSight(from, to);
    }
    private bool IsSameTeam(IDamagable damageable)
    {
        return _activeUnit.IsBlueTeam == damageable.IsBlueTeam;
    }
    public bool CanShowPreview(ActionContext ctx)
    {
        return ctx.TargetObject !=null;
    }

    public void Execute(ActionContext ctx)
    {
        if(_activeUnit is IDamageSource source)
        {
            source.SendDamage(new(ctx.TargetObject));
        }
    }

    public void ShowPreview(ActionContext ctx)
    {
        if (!IsSameTeam(ctx.TargetObject))
        {
            _cursorService.SetCursorState(CursorState.Default);
        }
        if (_activeUnit is IDamageSource source)
        {
            AttackContext attackContext = new(ctx.TargetObject);
            DamageContext damageContext = source.SimulateSendDamage(attackContext);
            var info = new AttackPreviewInfo
            {
                TargetPosition = ctx.TargetCell,
                DamageContext = damageContext,
                Description = damageContext.DamageAmount.ToString(),
            };
            _attackPanel.Show(info);
            if (IsSameTeam(ctx.TargetObject))
                _cursorService.SetCursorState(CursorState.Default);
            else
                _cursorService.SetCursorState(CursorState.Attack);
        }
        var movaAvailableCells = _movementSystem.GetReachableCells(_activeUnit.Position, _activeUnit.Stats.MoveSpeed);
        AddPreview(new(){_activeUnit .Position},CellState.accessibleRoutePoint);
        AddPreview(movaAvailableCells,CellState.moveAvailable);
    }

    private void AddPreview(List<Vector2Int> cells,CellState state)
    {
        _preview[state] = cells.ToList();
        _renderer.SetStates(cells, state);
    }

    public void HidePreview()
    {
        foreach(var state in _preview)
        {
            _renderer.RemoveStates(state.Key);
        }
        _preview.Clear();
        _attackPanel.Hide();
    }
    public void ShowAvaiableTargetCells()
    {
        //var units = _gameModel.GetUnits();
        //List<Vector2Int> unitPositions = units.Select(x => x.Position.Value).ToList();
        //foreach (var unit in units)
        //{
        //    ActionContext ctx = new ActionContext() { TargetCell = unit.Position.Value, TargetObject = unit, AbilityUsed = null };
        //    if (!CanHandle(ctx))
        //    {
        //        unitPositions.Remove(unit.Position.Value);
        //    }
        //}
        //AddPreview(unitPositions, CellState.moveAvailable);
    }

}