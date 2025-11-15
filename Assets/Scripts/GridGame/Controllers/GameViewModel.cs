using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class GameViewModel : IDisposable, IGridViewModel, IWorldToCellProvider
{
    public event Action<int, int> GridInited;
    public event Action<UnitViewModel> UnitStatsRequested;
    public event Action<PreviewResult> PreviewChanged;
    public event Action<PreviewResult> PreviewUpdated;
    public event Action<DamageContextPreview> DamageContextPreviewChanged;
    public Action<UnitViewModel> UnitSpawned { get; set; }
    public IGridRenderSettings RenderSettings { get;set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly GameModel _gameModel;
    private readonly MovementSystem _movementSystem;
    private readonly IGameCommandExecutor _gameCommandExecutor;
    private readonly ITurnStateViewModel _turnState;
    private readonly ActionResolver _actionResolver;
    private readonly CompositeDisposable _subscriptions = new();
    private readonly Dictionary<IGridContent, UnitViewModel> _uvms = new();
    private readonly Dictionary<CellState, List<Vector2Int>> _data = new();
    private Vector2Int? _hoverOverride;

    public GameViewModel(
        GameModel model,
        MovementSystem movementSystem,
        IGameCommandExecutor actionExecutor,
        ITurnStateViewModel turnState,
        ActionResolver actionResolver,
        IGridRenderSettings gridRenderSettings)
    {
        _gameModel = model ?? throw new ArgumentNullException(nameof(model));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _gameCommandExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
        _turnState = turnState ?? throw new ArgumentNullException(nameof(turnState));
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        RenderSettings = gridRenderSettings ?? throw new ArgumentNullException(nameof(gridRenderSettings));

        _gameModel.GameChange_Initialized += OnGameModelGridInitialized;
        _gameModel.GameChange_UnitSpawned += OnGameModelUnitSpawned;
        _turnState.ActiveObject
            .Subscribe(OnActiveUnitChanged)
            .AddTo(_subscriptions);
    }
    public void HandleCellHovered(Vector2Int? cell)
    {
        if (cell.HasValue)
        {
            _hoverOverride = cell;
            UpdateHoverState(cell);
        }
        else
        {
            _hoverOverride = null;
            UpdateHoverState(null);
        }

        PreviewUpdated?.Invoke(new PreviewResult(_data));
    }

    public void HandleCellHovered(Vector2Int cell)
    {
        HandleCellHovered(cell, -Vector2Int.one);
    }

    public void HandleCellHovered(Vector2Int cell, Vector2Int nearestCell)
    {
        if (cell.y < 0 || cell.x < 0)
            return;
        if (_hoverOverride.HasValue)
        {
            return;
        }

        var activeUnit = _turnState.ActiveObject.Value;
        if (activeUnit == null)
            return;

        UpdateHoverState(cell);

        var previewResult = new PreviewResult();
        var enemyPreviewAdded = TryAddEnemyReachablePreview(previewResult, cell);
        if (_data.TryGetValue(CellState.reachableCell, out var reachableCells) && reachableCells != null && reachableCells.Count > 0)
        {
            previewResult.Add(CellState.reachableCell, reachableCells);
        }
        previewResult.Add(CellState.hovered, new[] { cell });

        if (_turnState.IsMyTurn)
        {
            var fromCell = activeUnit.Position;
            var actionContext = new ActionContext(fromCell, cell, SpellType.None, nearestCell);

            _actionResolver.Resolve(actionContext, out var actionHandler);
            if (actionHandler != null)
            {
                previewResult.Add(actionHandler.GetPreview(actionContext).ToDictionary());
                if (actionHandler is IAttackActionHandler attackHandler)
                {
                    DamageContextPreviewChanged?.Invoke(new(attackHandler.GetDamageContext(actionContext)));
                }
                else
                {
                    DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
                }
            }
            else
            {
                previewResult.Add(CellState.accessibleRoutePoint, Array.Empty<Vector2Int>());
                previewResult.Add(CellState.inaccessibleRoutePoint, Array.Empty<Vector2Int>());
                DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
            }
        }
        else
        {
            if (!enemyPreviewAdded)
            {
                var fromCell = activeUnit.Position;
                var unit = _gameModel.GetCell(fromCell).Unit;
                if (unit != null)
                {
                    var cells = _movementSystem.GetReachableCells(fromCell, unit.ModifiedStats.MoveSpeed);
                    previewResult.Add(CellState.enemyReachableCell, cells);
                    enemyPreviewAdded = true;
                }
            }
        }

        if (!enemyPreviewAdded)
        {
            previewResult.Add(CellState.enemyReachableCell, Array.Empty<Vector2Int>());
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
        if (actionHandler == null)
        {
            Debug.LogWarning($"[GameViewModel] No action handler for {coords.Key}");
            return;
        }

        if (!actionHandler.CanExecute(actionContext))
        {
            Debug.LogWarning($"[GameViewModel] Action {actionHandler.ActionType} rejected locally for context {actionContext}");
            return;
        }

        if (!_gameCommandExecutor.Execute(actionHandler.ActionType, actionContext))
        {
            Debug.LogWarning($"[GameViewModel] Command executor declined {actionHandler.ActionType}");
        }
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

    public void HandleCellActionPerformed(Vector2Int cell, Vector2Int nearestCell)
    {
        HandleCellActionPerformed(new KeyValuePair<Vector2Int, Vector2Int>(cell, nearestCell));
    }
    protected virtual void OnGameModelUnitSpawned(UnitModelCreatedParams @params)
    {
        var unitVM = new UnitViewModel(@params.UnitModel, this);
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
        Width = grid.GetWidth();
        Height = grid.GetHeight();
        GridInited?.Invoke(Width, Height);
    }

    private void OnActiveUnitChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
            return;

        SetReachableCellState(combatObject);
        var previewResult = new PreviewResult(_data);
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

    private void UpdateHoverState(Vector2Int? cell)
    {
        if (cell.HasValue)
        {
            _data[CellState.hovered] = new List<Vector2Int> { cell.Value };
        }
        else
        {
            _data[CellState.hovered] = new List<Vector2Int>();
        }
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

    public void SetCell(UnitViewModel draggedVM, Vector2Int coords)
    {
        var model = _uvms.First(kvp => kvp.Value == draggedVM).Key;
        _gameModel.MoveObject(model, coords);
    }

   

    public Vector3 ToWorld(int x, int y)
    {
        var step = RenderSettings.CellSize + RenderSettings.CellPadding;
        return new Vector3(x * step, 0f, y * step);
    }

    public bool ToGrid(Vector3 position, out Vector2Int coords)
    {
        var step = RenderSettings.CellSize + RenderSettings.CellPadding;
        coords = new Vector2Int(
            Mathf.RoundToInt(position.x / step),
            Mathf.RoundToInt(position.z / step));
        return true;
    }

    public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        var success = ToGrid(position, out var main);
        coords = new KeyValuePair<Vector2Int, Vector2Int>(main, main);
        return success;
    }
    private bool TryAddEnemyReachablePreview(PreviewResult preview, Vector2Int cell)
    {
        var gridCell = _gameModel.GetCell(cell);
        if (gridCell == null)
            return false;

        var unit = gridCell.Unit as UnitModel;
        if (unit == null || unit.Team.Value == _turnState.LocalTeam)
            return false;

        var cells = _movementSystem.GetReachableCells(unit.Position.Value, unit.ModifiedStats.MoveSpeed);
        preview.Add(CellState.enemyReachableCell, cells);
        return true;
    }
}
   
