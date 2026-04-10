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
    public event Action<UnitViewModel, UnitViewModel> AttackAnimationRequested;
    public event Action<UnitViewModel> HoveredUnitChanged;
    public Action<UnitViewModel> UnitSpawned { get; set; }
    public IGridRenderSettings RenderSettings { get;set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    private const int DeploymentRows = 2;

    private readonly GameModel _gameModel;
    private readonly MovementSystem _movementSystem;
    private readonly IGameCommandExecutor _gameCommandExecutor;
    private readonly ITurnStateViewModel _turnState;
    private readonly IBattleControlModeService _battleControlModes;
    private readonly IBattleAnimationGate _animationGate;
    private readonly IGameConfigurationService _gameConfigurationService;
    private readonly ActionResolver _actionResolver;
    private readonly CompositeDisposable _subscriptions = new();
    private readonly Dictionary<IGridContent, UnitViewModel> _uvms = new();
    private readonly Dictionary<CellState, List<Vector2Int>> _data = new();
    private Vector2Int? _hoverOverride;
    private UnitViewModel _hoveredUnit;

    public GameViewModel(
        GameModel model,
        MovementSystem movementSystem,
        IGameCommandExecutor actionExecutor,
        ITurnStateViewModel turnState,
        ActionResolver actionResolver,
        IGameConfigurationService gameConfigurationService,
        IGridRenderSettings gridRenderSettings,
        [InjectOptional] IBattleControlModeService battleControlModes = null,
        [InjectOptional] IBattleAnimationGate animationGate = null)
    {
        _gameModel = model ?? throw new ArgumentNullException(nameof(model));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _gameCommandExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
        _turnState = turnState ?? throw new ArgumentNullException(nameof(turnState));
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        _gameConfigurationService = gameConfigurationService ?? throw new ArgumentNullException(nameof(gameConfigurationService));
        RenderSettings = gridRenderSettings ?? throw new ArgumentNullException(nameof(gridRenderSettings));
        _battleControlModes = battleControlModes;
        _animationGate = animationGate;

        _gameModel.GameChange_Initialized += OnGameModelGridInitialized;
        _gameModel.GameChange_UnitSpawned += OnGameModelUnitSpawned;
        _turnState.ActiveObject
            .Subscribe(OnActiveUnitChanged)
            .AddTo(_subscriptions);
        _turnState.BattleStateProperty
            .Subscribe(OnBattleStateChanged)
            .AddTo(_subscriptions);
        _turnState.TurnNumber
            .Subscribe(OnTurnNumberChanged)
            .AddTo(_subscriptions);

        if (_battleControlModes != null)
        {
            _battleControlModes.TeamModeChanged
                .Subscribe(_ => RefreshCurrentTurnPreview())
                .AddTo(_subscriptions);
        }

        if (_animationGate != null)
        {
            Observable.FromEvent<Action<bool>, bool>(
                    handler => isLocked => handler(isLocked),
                    handler => _animationGate.LockStateChanged += handler,
                    handler => _animationGate.LockStateChanged -= handler)
                .Where(isLocked => !isLocked)
                .Subscribe(_ => RefreshCurrentTurnPreview())
                .AddTo(_subscriptions);
        }
    }
    public void HandleCellHovered(Vector2Int? cell)
    {
        if (cell.HasValue)
        {
            _hoverOverride = cell;
            UpdateHoverState(cell);
            SetHoveredUnit(cell.Value);
        }
        else
        {
            _hoverOverride = null;
            UpdateHoverState(null);
            SetHoveredUnit((UnitModel)null);
        }

        var previewResult = new PreviewResult();
        InitializePreviewStates(previewResult);

        if (_data.TryGetValue(CellState.reachableCell, out var reachableCells) && reachableCells != null && reachableCells.Count > 0)
        {
            previewResult.Add(CellState.reachableCell, reachableCells);
        }

        if (_data.TryGetValue(CellState.activeUnit, out var activeCells) && activeCells != null && activeCells.Count > 0)
        {
            previewResult.Add(CellState.activeUnit, activeCells);
        }

        if (_data.TryGetValue(CellState.hovered, out var hoveredCells) && hoveredCells != null && hoveredCells.Count > 0)
        {
            previewResult.Add(CellState.hovered, hoveredCells);
        }

        if (_data.TryGetValue(CellState.enemyCell, out var enemyUnitCells) && enemyUnitCells != null && enemyUnitCells.Count > 0)
        {
            previewResult.Add(CellState.enemyCell, enemyUnitCells);
        }

        PreviewUpdated?.Invoke(previewResult);
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

        var previewResult = new PreviewResult();
        InitializePreviewStates(previewResult);
        var hoveredCell = _gameModel.GetCell(cell);
        var hoveredUnit = hoveredCell?.Unit as UnitModel;
        bool enemyReachableAdded = false;
        SetHoveredUnit(hoveredUnit);

        if (_turnState.BattleStateProperty.Value == BattleState.replacement)
        {
            UpdateHoverState(null);
            SetHoveredUnit((UnitModel)null);
            ClearEnemyReachablePreview(previewResult);
            previewResult.Set(CellState.reachableCell, Array.Empty<Vector2Int>());
            DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
            PreviewUpdated?.Invoke(previewResult);
            return;
        }

        if (!_turnState.IsMyTurn)
        {
            UpdateHoverState(null);

            if (hoveredUnit != null)
            {
                enemyReachableAdded = TryAddUnitReachablePreview(previewResult, hoveredUnit);
            }

            if (!enemyReachableAdded)
            {
                ClearEnemyReachablePreview(previewResult);
            }
            previewResult.Set(CellState.reachableCell, Array.Empty<Vector2Int>());

            DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
            PreviewUpdated?.Invoke(previewResult);
            return;
        }

        if (_data.TryGetValue(CellState.reachableCell, out var reachableCells) && reachableCells != null && reachableCells.Count > 0)
        {
            previewResult.Add(CellState.reachableCell, reachableCells);
        }

        if (_data.TryGetValue(CellState.activeUnit, out var activeCells) && activeCells != null && activeCells.Count > 0)
        {
            previewResult.Add(CellState.activeUnit, activeCells);
        }

        if (_data.TryGetValue(CellState.enemyCell, out var enemyCells) && enemyCells != null && enemyCells.Count > 0)
        {
            previewResult.Add(CellState.enemyCell, enemyCells);
        }

        if (hoveredUnit == null)
        {
            UpdateHoverState(cell);
            previewResult.Add(CellState.hovered, new[] { cell });

            var fromCell = activeUnit.Position;
            var actionContext = new ActionContext(fromCell, cell, SpellType.None, cell);
            var moveHandler = _actionResolver.Resolve(ActionType.Move, actionContext);
            previewResult.Add(moveHandler.GetPreview(actionContext).ToDictionary());
            ClearEnemyReachablePreview(previewResult);
            DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
            PreviewUpdated?.Invoke(previewResult);
            return;
        }

        if (hoveredUnit.Team.Value == activeUnit.Team)
        {
            UpdateHoverState(null);
            if (!ReferenceEquals(hoveredUnit, activeUnit))
            {
                enemyReachableAdded = TryAddUnitReachablePreview(previewResult, hoveredUnit);
            }
            if (!enemyReachableAdded)
            {
                ClearEnemyReachablePreview(previewResult);
            }

            DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
            PreviewUpdated?.Invoke(previewResult);
            return;
        }

        UpdateHoverState(cell);
        previewResult.Add(CellState.hovered, new[] { cell });
        previewResult.Add(CellState.hoveredEnemy, new[] { cell });
        enemyReachableAdded = TryAddUnitReachablePreview(previewResult, hoveredUnit);

        var attackFromCell = ResolveAttackFromCell(activeUnit, cell, nearestCell);
        if (attackFromCell != activeUnit.Position)
        {
            var moveContext = new ActionContext(activeUnit.Position, attackFromCell, SpellType.None, attackFromCell);
            var moveHandler = _actionResolver.Resolve(ActionType.Move, moveContext);
            previewResult.Add(moveHandler.GetPreview(moveContext).ToDictionary());
        }

        var attackContext = new ActionContext(activeUnit.Position, cell, SpellType.None, attackFromCell);
        if (_actionResolver.TryResolvePlan(attackContext, out var plan) &&
            _actionResolver.Resolve(plan.ActionType, plan.Context) is IAttackActionHandler attackHandler)
        {
            DamageContextPreviewChanged?.Invoke(new(attackHandler.GetDamageContext(plan.Context)));
        }
        else
        {
            DamageContextPreviewChanged?.Invoke(DamageContextPreview.Empty);
        }

        if (!enemyReachableAdded)
        {
            ClearEnemyReachablePreview(previewResult);
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
        if (!_actionResolver.TryResolvePlan(actionContext, out var plan))
        {
            Debug.LogWarning($"[GameViewModel] No action handler for {coords.Key}");
            return;
        }
        var actionHandler = _actionResolver.Resolve(plan.ActionType, plan.Context);

        if (actionHandler is IAttackActionHandler)
        {
            RaiseAttackAnimationRequested(activeUnit, coords.Key);
        }

        if (!_gameCommandExecutor.Execute(actionHandler.ActionType, plan.Context))
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
        if (_turnState.BattleStateProperty.Value == BattleState.replacement)
        {
            ApplyDeploymentPreview();
        }
    }

    private void OnActiveUnitChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
            return;

        if (_turnState.BattleStateProperty.Value == BattleState.replacement)
        {
            ApplyDeploymentPreview();
            return;
        }

        SetReachableCellState(combatObject);
        SetEnemyCellState();
        _data[CellState.activeUnit] = new List<Vector2Int> { combatObject.Position };
        var previewResult = new PreviewResult(_data);
        PreviewChanged?.Invoke(previewResult);
    }

    private void OnBattleStateChanged(BattleState state)
    {
        if (state == BattleState.replacement)
        {
            ApplyDeploymentPreview();
            return;
        }

        if (_turnState.ActiveObject.Value != null)
        {
            OnActiveUnitChanged(_turnState.ActiveObject.Value);
        }
    }

    private void OnTurnNumberChanged(int _)
    {
        if (_turnState.BattleStateProperty.Value == BattleState.replacement)
        {
            ApplyDeploymentPreview();
            return;
        }

        if (_turnState.ActiveObject.Value != null)
        {
            OnActiveUnitChanged(_turnState.ActiveObject.Value);
        }
    }

    private void ApplyDeploymentPreview()
    {
        if (Width <= 0 || Height <= 0)
            return;

        _data[CellState.activeUnit] = new List<Vector2Int>();
        _data[CellState.enemyCell] = new List<Vector2Int>();
        _data[CellState.reachableCell] = BuildDeploymentCells(_turnState.LocalTeam);

        var previewResult = new PreviewResult(_data);
        PreviewChanged?.Invoke(previewResult);
    }

    private List<Vector2Int> BuildDeploymentCells(Team team)
    {
        var rows = Mathf.Clamp(DeploymentRows, 1, Height);
        var result = new List<Vector2Int>();

        if (!IsBottomTeam(team))
        {
            for (var y = Height - rows; y < Height; y++)
            for (var x = 0; x < Width; x++)
                result.Add(new Vector2Int(x, y));
        }
        else
        {
            for (var y = 0; y < rows; y++)
            for (var x = 0; x < Width; x++)
                result.Add(new Vector2Int(x, y));
        }

        return result;
    }

    private void SetReachableCellState(ICombatObject combatObject)
    {
        var reachableCells = new List<Vector2Int>();
        if (_turnState.IsMyTurn)
        {
            var cells = _movementSystem.GetReachableCells(combatObject.Position, combatObject.Stats.MoveSpeed);
            reachableCells = ShouldApplyDeploymentBounds()
                ? ApplyDeploymentBounds(combatObject.Team, combatObject.Position, cells)
                : new List<Vector2Int>(cells);
        }

        _data[CellState.reachableCell] = reachableCells;
    }

    private void SetEnemyCellState()
    {
        if (!_turnState.IsMyTurn)
        {
            _data[CellState.enemyCell] = new List<Vector2Int>();
            return;
        }

        var activeTeam = _turnState.ActiveObject.Value?.Team ?? _turnState.LocalTeam;
        _data[CellState.enemyCell] = GetEnemyCells(activeTeam);
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

    private void RaiseAttackAnimationRequested(IGridContent attacker, Vector2Int targetCell)
    {
        if (attacker == null)
            return;

        var targetContent = _gameModel.GetCell(targetCell)?.Unit;
        if (targetContent == null)
            return;

        if (!_uvms.TryGetValue(attacker, out var attackerVm))
            return;

        if (!_uvms.TryGetValue(targetContent, out var defenderVm))
            return;

        AttackAnimationRequested?.Invoke(attackerVm, defenderVm);
    }

    public bool CanExecute(ActionType type, ActionContext actionContext)
    {
        return _actionResolver.TryResolvePlan(actionContext, out var plan) && plan.ActionType == type;
    }

    public void Execute(ActionType type, ActionContext actionContext)
    {
        if (!_actionResolver.TryResolvePlan(actionContext, out var plan) || plan.ActionType != type)
            throw new InvalidOperationException("Action cannot be executed for supplied context");

        var handler = _actionResolver.Resolve(plan.ActionType, plan.Context);
        handler.Execute(plan.Context);
    }

    public bool TrySetCell(UnitViewModel draggedVM, Vector2Int coords)
    {
        if (draggedVM == null)
            return false;
        if (!IsInBounds(coords))
            return false;
        if (_turnState.BattleStateProperty.Value != BattleState.replacement)
            return false;

        var kvp = _uvms.FirstOrDefault(pair => pair.Value == draggedVM);
        var model = kvp.Key;
        if (model == null)
            return false;

        if (model.Team != _turnState.LocalTeam)
            return false;

        if (!IsWithinDeploymentZone(model.Team, coords))
            return false;

        if (IsCellOccupied(coords) && model.Position != coords)
            return false;

        return _gameCommandExecutor.TryDeployUnit(model.Position, coords);
    }

    public void SetCell(UnitViewModel draggedVM, Vector2Int coords)
    {
        TrySetCell(draggedVM, coords);
    }

    public bool IsInBounds(Vector2Int coords)
    {
        if (Width <= 0 || Height <= 0)
            return false;
        return coords.x >= 0 && coords.x < Width && coords.y >= 0 && coords.y < Height;
    }

    public bool TryGetUnitCell(UnitViewModel vm, out Vector2Int coords)
    {
        coords = default;
        if (vm == null)
            return false;

        foreach (var kvp in _uvms)
        {
            if (kvp.Value == vm && kvp.Key != null)
            {
                coords = kvp.Key.Position;
                return true;
            }
        }

        return false;
    }

    public bool TryGetUnitAtCell(Vector2Int cell, out UnitViewModel viewModel)
    {
        viewModel = null;
        var cellObj = _gameModel.GetCell(cell);
        var unit = cellObj?.Unit;
        if (unit == null)
            return false;

        return _uvms.TryGetValue(unit, out viewModel);
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        var cellObj = _gameModel.GetCell(cell);
        return cellObj?.Unit != null;
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
    private bool TryAddUnitReachablePreview(PreviewResult preview, UnitModel unit)
    {
        if (unit == null || unit.ModifiedStats == null)
            return false;

        var cells = _movementSystem.GetReachableCells(unit.Position.Value, unit.ModifiedStats.MoveSpeed);
        var filtered = ShouldApplyDeploymentBounds()
            ? ApplyDeploymentBounds(unit.Team.Value, unit.Position.Value, cells)
            : new List<Vector2Int>(cells);
        preview.Add(CellState.enemyReachableCell, filtered);
        return true;
    }

    private bool ShouldApplyDeploymentBounds()
    {
        return _turnState.BattleStateProperty.Value == BattleState.replacement;
    }

    private void ClearEnemyReachablePreview(PreviewResult preview)
    {
        preview.Set(CellState.enemyReachableCell, Array.Empty<Vector2Int>());
    }

    private List<Vector2Int> GetEnemyCells(Team activeTeam)
    {
        var result = new List<Vector2Int>();
        foreach (var unit in _turnState.CombatUnits)
        {
            if (unit == null || unit.Team == activeTeam)
                continue;
            result.Add(unit.Position);
        }
        return result;
    }

    private void RefreshCurrentTurnPreview()
    {
        if (_turnState.BattleStateProperty.Value == BattleState.replacement)
        {
            ApplyDeploymentPreview();
            return;
        }

        if (_turnState.ActiveObject.Value != null)
        {
            OnActiveUnitChanged(_turnState.ActiveObject.Value);
        }
    }

    private static void InitializePreviewStates(PreviewResult preview)
    {
        preview.Set(CellState.hovered, Array.Empty<Vector2Int>());
        preview.Set(CellState.activeUnit, Array.Empty<Vector2Int>());
        preview.Set(CellState.hoveredEnemy, Array.Empty<Vector2Int>());
        preview.Set(CellState.reachableCell, Array.Empty<Vector2Int>());
        preview.Set(CellState.enemyReachableCell, Array.Empty<Vector2Int>());
        preview.Set(CellState.attackTarget, Array.Empty<Vector2Int>());
        preview.Set(CellState.attackTargetBlocked, Array.Empty<Vector2Int>());
        preview.Set(CellState.enemyCell, Array.Empty<Vector2Int>());
        preview.Set(CellState.accessibleRoutePoint, Array.Empty<Vector2Int>());
        preview.Set(CellState.inaccessibleRoutePoint, Array.Empty<Vector2Int>());
        preview.Set(CellState.routeEndAccessible, Array.Empty<Vector2Int>());
        preview.Set(CellState.routeEndBlocked, Array.Empty<Vector2Int>());
    }

    private void SetHoveredUnit(Vector2Int cell)
    {
        var hoveredCell = _gameModel.GetCell(cell);
        var hoveredUnit = hoveredCell?.Unit as UnitModel;
        SetHoveredUnit(hoveredUnit);
    }

    private void SetHoveredUnit(UnitModel unit)
    {
        UnitViewModel hoveredVm = null;
        if (unit != null)
        {
            _uvms.TryGetValue(unit, out hoveredVm);
        }

        if (_hoveredUnit == hoveredVm)
            return;

        _hoveredUnit = hoveredVm;
        HoveredUnitChanged?.Invoke(_hoveredUnit);
    }

    private Vector2Int ResolveAttackFromCell(ICombatObject attacker, Vector2Int targetCell, Vector2Int nearestCell)
    {
        if (nearestCell.x >= 0 && nearestCell.y >= 0)
            return nearestCell;

        if (attacker is not UnitModel unit || unit.ModifiedStats == null)
            return attacker.Position;

        if (TryFindAttackFromCell(unit, targetCell, out var attackFrom))
        {
            return attackFrom;
        }

        return attacker.Position;
    }

    private bool TryFindAttackFromCell(UnitModel attacker, Vector2Int targetCell, out Vector2Int attackFrom)
    {
        attackFrom = attacker.Position.Value;
        var stats = attacker.ModifiedStats;
        if (stats == null)
            return false;
        if (stats.AttackRange > 1)
            return false;

        var reachable = _movementSystem.GetReachableCells(attacker.Position.Value, stats.MoveSpeed);
        if (reachable == null || reachable.Count == 0)
            return false;

        float bestCost = float.MaxValue;
        Vector2Int bestCell = attacker.Position.Value;

        foreach (var cell in reachable)
        {
            var cellObj = _gameModel.GetCell(cell);
            if (cellObj == null || !cellObj.IsEmpty)
                continue;

            if (!_movementSystem.GetRouteIgnoringObstacles(cell, targetCell, out var attackRoute))
                continue;

            var attackDistance = _movementSystem.GetRouteCost(attackRoute);
            if (attackDistance > stats.AttackRange)
                continue;

            if (!_movementSystem.GetRoute(attacker.Position.Value, cell, out var moveRoute))
                continue;

            var moveCost = _movementSystem.GetRouteCost(moveRoute);
            if (moveCost <= stats.MoveSpeed && moveCost < bestCost)
            {
                bestCost = moveCost;
                bestCell = cell;
            }
        }

        if (bestCost < float.MaxValue)
        {
            attackFrom = bestCell;
            return true;
        }

        return false;
    }

    private List<Vector2Int> ApplyDeploymentBounds(Team team, Vector2Int origin, IEnumerable<Vector2Int> cells)
    {
        if (cells == null)
            return new List<Vector2Int> { origin };

        if (Height <= 0 || team == Team.None)
            return new List<Vector2Int>(cells);

        var result = new List<Vector2Int>();
        foreach (var cell in cells)
        {
            if (IsWithinDeploymentZone(team, cell))
            {
                if (!result.Contains(cell))
                {
                    result.Add(cell);
                }
            }
        }

        if (!result.Contains(origin))
        {
            result.Insert(0, origin);
        }

        return result;
    }

    private bool IsWithinDeploymentZone(Team team, Vector2Int cell)
    {
        if (Height <= 0 || team == Team.None)
            return true;

        var rows = Mathf.Clamp(DeploymentRows, 1, Height);

        return !IsBottomTeam(team)
            ? cell.y >= Height - rows
            : cell.y < rows;
    }

    private bool IsBottomTeam(Team team)
    {
        var bottomTeam = _gameConfigurationService?.BattlefieldBottomTeam ?? Team.Blue;
        return team == bottomTeam;
    }
}
   
