using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
/// <summary>
/// Модель представления сетки. Зависит от текущего активного юнита(модель), а также клетки, на которую наведен курсор(модель представления), а также состояния отображения - если играется анимация, то всё скрыть. 
/// </summary>
public class GridViewModel
{
    private readonly ITurnStateViewModel _turnState;
    private readonly ActionResolver _actionResolver;
    private readonly MovementSystem _movementSystem;
    private readonly CompositeDisposable _subscriptions = new();
    public Action<PreviewResult> PreviewChanged { get; set; }
    private readonly Dictionary<CellState, List<Vector2Int>> _data = new();
    public Action<GridXZ<GameCell>> GridInited { get; set; }
    [Inject]
    public GridViewModel(ITurnStateViewModel turnState, ActionResolver actionResolver, MovementSystem movementSystem, GameModel model )
    {
        _turnState = turnState;
        _actionResolver = actionResolver;
        _movementSystem = movementSystem;

        _turnState.ActiveObject
            .Subscribe(ActiveObjectChanged)
            .AddTo(_subscriptions);

        _actionResolver.ActionResolved += OnActionResolved;
        model.GameChange_Initialized += OnGameModel_GridInilized;
    }

    private void OnGameModel_GridInilized(GridXZ<GameCell> xZ)
    {
        GridInited?.Invoke(xZ);
    }

    private void ActiveObjectChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
            return;
        PreviewResult previewResult = new();
        if (_turnState.IsMyTurn)
        {
            var reachableCells = _movementSystem.GetReachableCells(combatObject.Position, combatObject.Stats.MoveSpeed);
            previewResult.Add(CellState.reachableCell, new List<Vector2Int>(reachableCells));
        }
        else
        {
        }
            PreviewChanged?.Invoke(previewResult);
    }
    private void OnActionResolved((IActionHandler handler,ActionContext ctx) pair)
    {
        Debug.LogError("ActionResolved");
        var preview = pair.handler.GetPreview(pair.ctx);
        PreviewChanged?.Invoke(preview);
    }
}
