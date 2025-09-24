using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
/// <summary>
/// Модель представления сетки. Зависит от текущего активного юнита(модель), а также клетки, на которую наведен курсор(модель представления), а также состояния отображения - если играется анимация, то всё скрыть. 
/// </summary>
public class GridViewModel : IGridViewModel
{
    TurnSystem _turnSystem;
    ActionResolver _actionResolver;
    MovementSystem _movementSystem;
    public Action<PreviewResult> PreviewChanged { get; set; }
    public GridViewModel(TurnSystem turnSystem, ActionResolver actionResolver, MovementSystem movementSystem)
    {
        _turnSystem = turnSystem;
        _actionResolver = actionResolver;
        _movementSystem = movementSystem;

        _turnSystem.ActiveObject.Subscribe(ActiveObjectChanged);
        _actionResolver.ActionResolved += OnActionResolved;
    }

    private void ActiveObjectChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
            return;
        PreviewResult previewResult = new();
        if (_turnSystem.IsMyTurn)
        {
            var reachableCells = _movementSystem.GetReachableCells(combatObject.Position, combatObject.Stats.MoveSpeed);
            previewResult.Add(CellState.reachableCell, new List<Vector2Int>(reachableCells));
        }
        PreviewChanged?.Invoke(previewResult);
    }
    private void OnActionResolved((IActionHandler handler,ActionContext ctx) pair)
    {
        var preview = pair.handler.GetPreview(pair.ctx);
        PreviewChanged?.Invoke(preview);
    }
}