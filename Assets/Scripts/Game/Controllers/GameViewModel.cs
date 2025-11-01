using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class GameViewModel : IDisposable, IGridViewModel
{
    public event Action<int, int> GridInited;
    public event Action<UnitViewModel> UnitStatsRequested;
    public event Action<PreviewResult> PreviewChanged;
    public event Action<PreviewResult> PreviewUpdated;
    public event Action<DamageContextPreview> DamageContextPreviewChanged;
    public Action<UnitViewModel> UnitSpawned;

    private readonly GameModel _gameModel;
    private readonly MovementSystem _movementSystem;
    private readonly IGameCommandExecutor _gameCommandExecutor;
    private readonly ITurnStateViewModel _turnState;
    private readonly ActionResolver _actionResolver;
    private readonly CompositeDisposable _subscriptions = new();
    private readonly Dictionary<IGridContent, UnitViewModel> _uvms = new();
    private readonly Dictionary<CellState, List<Vector2Int>> _data = new();

    public GameViewModel(
        GameModel model,
        MovementSystem movementSystem,
        IGameCommandExecutor actionExecutor,
        ITurnStateViewModel turnState,
        ActionResolver actionResolver)
    {
        _gameModel = model ?? throw new ArgumentNullException(nameof(model));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _gameCommandExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
        _turnState = turnState ?? throw new ArgumentNullException(nameof(turnState));
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));

        _gameModel.GameChange_Initialized += OnGameModelGridInitialized;
        _gameModel.GameChange_UnitSpawned += OnGameModelUnitSpawned;
        _turnState.ActiveObject
            .Subscribe(OnActiveUnitChanged)
            .AddTo(_subscriptions);
    }

    public void HandleCellHovered(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        var activeUnit = _turnState.ActiveObject.Value;
        if (activeUnit == null)
            return;

        _data[CellState.hovered] = new List<Vector2Int> { coords.Key };
        var previewResult = new PreviewResult();
        previewResult.Add(CellState.hovered, new[] { coords.Key });

        if (_turnState.IsMyTurn)
        {
            var fromCell = activeUnit.Position;
            var actionContext = new ActionContext(fromCell, coords.Key, SpellType.None, coords.Value);

            _actionResolver.Resolve(actionContext, out var actionHandler);
            if (actionHandler != null)
            {
                previewResult.Add(actionHandler.GetPreview(actionContext).ToDictionary());
                if (actionHandler is IAttackActionHandler attackHandler)
                {
                    DamageContextPreviewChanged?.Invoke(attackHandler.GetDamagePreview(actionContext));
                }
                else
                {
                    DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
                }
            }
            else
            {
                previewResult.Add(CellState.reachableCell, Array.Empty<Vector2Int>());
                previewResult.Add(CellState.accessibleRoutePoint, Array.Empty<Vector2Int>());
                previewResult.Add(CellState.inaccessibleRoutePoint, Array.Empty<Vector2Int>());
                DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
            }
        }
        else
        {
            var fromCell = activeUnit.Position;
            var unit = _gameModel.GetCell(fromCell).Unit;
            if (unit != null)
            {
                var cells = _movementSystem.GetReachableCells(fromCell, unit.ModifiedStats.MoveSpeed);
                previewResult.Add(CellState.enemyReachableCell, cells);
            }
            else
            {
                previewResult.Add(CellState.enemyReachableCell, Array.Empty<Vector2Int>());
            }
        }

        PreviewUpdated?.Invoke(previewResult);
    }

    public void HandleCellSelected(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        if (!_turnState.IsMyTurn)
            return;

        var activeUnit = _turnState.ActiveObject.Value;
        if (activeUnit == null)
            return;

        var fromCell = activeUnit.Position;
        var actionContext = new ActionContext(fromCell, coords.Key, SpellType.None, coords.Value);
        _actionResolver.Resolve(actionContext, out var actionHandler);
        _gameCommandExecutor.Execute(actionHandler.ActionType, actionContext);
    }

    public void HandleCellActionPerformed(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        var cell = _gameModel.GetCell(coords.Key);
        var unit = cell.Unit;
        if (unit != null && _uvms.TryGetValue(unit, out var vm))
        {
            UnitStatsRequested?.Invoke(vm);
        }
    }

    protected virtual void OnGameModelUnitSpawned(UnitModelCreatedParams @params)
    {
        var unitVM = new UnitViewModel(@params.UnitModel);
        _uvms[@params.UnitModel] = unitVM;
        UnitSpawned?.Invoke(unitVM);
    }

    public void Dispose()
    {
        _gameModel.GameChange_Initialized -= OnGameModelGridInitialized;
        _gameModel.GameChange_UnitSpawned -= OnGameModelUnitSpawned;
        _subscriptions.Dispose();
    }

    private void OnGameModelGridInitialized(GridXZ<GameCell> grid)
    {
        GridInited?.Invoke(grid.GetWidth(), grid.GetHeight());
    }

    private void OnActiveUnitChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
            return;

        var previewResult = new PreviewResult(_data);
        SetReachableCellState(combatObject);
        PreviewChanged?.Invoke(previewResult);
    }

    private void SetReachableCellState(ICombatObject combatObject)
    {
        var reachableCells = new List<Vector2Int>();
        if (_turnState.IsMyTurn)
        {
            reachableCells = _movementSystem.GetReachableCells(combatObject.Position, combatObject.Stats.MoveSpeed);
        }

        _data[CellState.reachableCell] = reachableCells;
    }

    public bool CanExecute(ActionType type, ActionContext actionContext)
    {
        var handler = _actionResolver.Resolve(type, actionContext);
        return handler.CanExecute(actionContext);
    }

    public void Execute(ActionType type, ActionContext actionContext)
    {
        var handler = _actionResolver.Resolve(type, actionContext);
        if (!handler.CanExecute(actionContext))
            throw new InvalidOperationException("Action cannot be executed for supplied context");

        handler.Execute(actionContext);
    }

    public void SnapToCell(UnitViewModel draggedVM, Vector2Int coords)
    {
        var model = _uvms.First(kvp => kvp.Value == draggedVM).Key;
        _gameModel.MoveObject(model, coords);
    }
}
