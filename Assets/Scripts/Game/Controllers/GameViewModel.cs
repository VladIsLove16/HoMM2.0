using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class GameViewModel : IDisposable, IGridViewModel
{
    public event Action<int,int> GridInited;
    public event Action<UnitViewModel> UnitStatsRequested;
    public event Action<PreviewResult> PreviewChanged;
    public event Action<PreviewResult> PreviewUpdated;
    public event Action<DamageContextPreview> DamageContextPreviewChanged;
    public Action<UnitViewModel> UnitSpawned;
    private readonly GameModel _gameModel;
    private readonly MovementSystem _movementSystem;
    private readonly IGameCommandExecutor _gameCommandExecutor;
    private TurnSystem _turnSystem;
    private readonly ActionResolver _actionResolver;
    private Dictionary<IGridContent, UnitViewModel> _uvms = new Dictionary<IGridContent, UnitViewModel>();
    private GameModel model;
    private Dictionary<CellState, List<Vector2Int>> _data = new();
    public GameViewModel(GameModel model, MovementSystem movementSystem, IGameCommandExecutor actionExecutor, TurnSystem turnSystem, ActionResolver actionResolver)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (movementSystem == null) throw new ArgumentNullException(nameof(movementSystem));
        if (actionExecutor == null) throw new ArgumentNullException(nameof(actionExecutor));
        if (turnSystem == null) throw new ArgumentNullException(nameof(turnSystem));
        if (actionResolver == null) throw new ArgumentNullException(nameof(actionResolver));

        _gameModel = model;
        _movementSystem = movementSystem;
        _gameCommandExecutor = actionExecutor;
        _turnSystem = turnSystem;
        _actionResolver = actionResolver;

        _gameModel.GameChange_Initialized += OnGameModel_GridInitilized;
        _gameModel.GameChange_UnitSpawned += OnGameModel_UnitSpawned;
        _turnSystem.ActiveObject.Subscribe(unit => OnActiveUnitChanged(unit));
    }
    public void HandleCellHovered(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        _data[CellState.hovered] = new List<Vector2Int>() { coords.Key };
        PreviewResult previewResult = new();
        previewResult.Add(CellState.hovered, new List<Vector2Int>() { coords.Key });
        if (_turnSystem.IsMyTurn)
        {
            var fromCell = _turnSystem.ActiveObject.Value.Position;
            ActionContext actionContext = new(fromCell, coords.Key, SpellType.None, coords.Value);
            _actionResolver.Resolve(actionContext, out var actionHandler);
            if (actionHandler == null)
                return;
            previewResult.Add(actionHandler.GetPreview(actionContext).ToDictionary());
            if (actionHandler is IAttackActionHandler attackActionHandler)
            {
                DamageContextPreviewChanged?.Invoke(attackActionHandler.GetDamagePreview(actionContext));
            }
        }
        else
        {
            var fromCell = _turnSystem.ActiveObject.Value.Position;
            var unit = _gameModel.GetCell(fromCell).Unit;
            if (unit != null)
            {
                var cells = _movementSystem.GetReachableCells(fromCell, unit.ModifiedStats.MoveSpeed);
                previewResult.Add(CellState.enemyReachableCell, cells);
            }
            else
                previewResult.Add(CellState.enemyReachableCell, new List<Vector2Int>());
        }
        PreviewUpdated?.Invoke(previewResult);
    }

    public void HandleCellSelected(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        Debug.Log("HandleCellSelected");
        if (!_turnSystem.IsMyTurn)
            return;
        var fromCell = _turnSystem.ActiveObject.Value.Position;
        ActionContext actionContext = new(fromCell, coords.Key, SpellType.None, coords.Value);
        _actionResolver.Resolve(actionContext, out var actionHandler);
        Debug.Log("resolved" + actionContext);
        _gameCommandExecutor.Execute(actionHandler.ActionType, actionContext);
    }

    public void HandleCellActionPerformed(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        var cell = _gameModel.GetCell(coords.Key);
        var unit = cell.Unit;
        if (_uvms.TryGetValue(unit,out var vm))
        {
            UnitStatsRequested?.Invoke(vm);
        }
    }
    protected virtual void OnGameModel_UnitSpawned(UnitModelCreatedParams @params)
    {
        UnitViewModel uvm = new(@params.UnitModel);
        _uvms[@params.UnitModel] = uvm;

        UnitSpawned?.Invoke(uvm);
    }

    public void Dispose()
    {
        _gameModel.GameChange_Initialized -= OnGameModel_GridInitilized;
        _gameModel.GameChange_UnitSpawned -= OnGameModel_UnitSpawned;
    }

    private void OnGameModel_GridInitilized(GridXZ<GameCell> g)
    {
        GridInited?.Invoke(g.GetWidth(),g.GetHeight());
    }

    private void OnActiveUnitChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
            return;
        PreviewResult previewResult = new(_data);
        SetReachableCellState(combatObject);
        PreviewChanged?.Invoke(previewResult);
    }

    private void SetReachableCellState(ICombatObject combatObject)
    {
        var reachableCells = new List<Vector2Int>();
        if (_turnSystem.IsMyTurn)
            reachableCells = _movementSystem.GetReachableCells(combatObject.Position, combatObject.Stats.MoveSpeed);
        _data[CellState.reachableCell] = reachableCells;

    }

    public bool CanExecute(ActionType type, ActionContext actionContext)
    {
        IActionHandler actionHandler = _actionResolver.Resolve(type, actionContext);
        return actionHandler.CanExecute(actionContext);
    }

    public void Execute(ActionType type, ActionContext actionContext)
    {
        IActionHandler actionHandler = _actionResolver.Resolve(type, actionContext);
        if (!actionHandler.CanExecute(actionContext))
            throw new Exception();
        actionHandler.Execute(actionContext);
    }

    public void SnapToCell(UnitViewModel draggedVM, Vector2Int coords)
    {
        var model = _uvms.First(UnitViewModel => UnitViewModel.Value == draggedVM).Key;
        _gameModel.MoveObject(model,coords);
    }
}
