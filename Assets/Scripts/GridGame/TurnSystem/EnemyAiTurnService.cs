using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public sealed class EnemyAiTurnService : IDisposable
{
    private readonly ITurnService _turnService;
    private readonly IBattleControlModeService _battleControlModes;
    private readonly IBattleAnimationGate _animationGate;
    private readonly ActionResolver _resolver;
    private readonly ActionPipeline _pipeline;
    private readonly MovementSystem _movementSystem;
    private readonly GameModel _gameModel;
    private readonly BattleAiControlConfigSO _config;
    private readonly CompositeDisposable _disposables = new();

    private bool _isProcessing;
    private bool _deploymentApplied;
    private IDisposable _pendingTurn;

    public EnemyAiTurnService(
        ITurnService turnService,
        IBattleControlModeService battleControlModes,
        IBattleAnimationGate animationGate,
        ActionResolver resolver,
        ActionPipeline pipeline,
        MovementSystem movementSystem,
        GameModel gameModel,
        BattleAiControlConfigSO config)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _battleControlModes = battleControlModes ?? throw new ArgumentNullException(nameof(battleControlModes));
        _animationGate = animationGate ?? throw new ArgumentNullException(nameof(animationGate));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _gameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _turnService.ActiveObjectStream
            .Subscribe(_ => ScheduleProcessingIfNeeded())
            .AddTo(_disposables);

        _turnService.BattleStateStream
            .Subscribe(OnBattleStateChanged)
            .AddTo(_disposables);

        _battleControlModes.TeamModeChanged
            .Subscribe(_ => ScheduleProcessingIfNeeded())
            .AddTo(_disposables);

        Observable.FromEvent<Action<bool>, bool>(
                handler => isLocked => handler(isLocked),
                handler => _animationGate.LockStateChanged += handler,
                handler => _animationGate.LockStateChanged -= handler)
            .Subscribe(OnAnimationGateStateChanged)
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _pendingTurn?.Dispose();
        _disposables.Dispose();
    }

    private void OnBattleStateChanged(BattleState state)
    {
        if (state == BattleState.replacement)
        {
            TryRandomizeDeployment();
        }
        else
        {
            _deploymentApplied = false;
        }

        ScheduleProcessingIfNeeded();
    }

    private void ScheduleProcessingIfNeeded()
    {
        _pendingTurn?.Dispose();
        _pendingTurn = null;

        if (!ShouldControlCurrentUnit())
            return;

        if (_animationGate.IsLocked)
        {
            UnityLogger.Log("[EnemyAiTurnService] AI scheduling deferred until battle animation completes.", LogCategory.AI);
            return;
        }

        var delaySeconds = _battleControlModes.IsFastResolveActive.Value ? 0f : _config.AiTurnDelaySeconds;
        if (delaySeconds <= 0f)
        {
            ProcessEnemyTurn();
            return;
        }

        UnityLogger.Log($"[EnemyAiTurnService] Scheduling AI action in {delaySeconds:0.00}s for {_turnService.ActiveObject}", LogCategory.AI);
        _pendingTurn = Observable.Timer(TimeSpan.FromSeconds(delaySeconds))
            .Subscribe(_ => ProcessEnemyTurn());
    }

    private void OnAnimationGateStateChanged(bool isLocked)
    {
        if (isLocked)
        {
            _pendingTurn?.Dispose();
            _pendingTurn = null;
            UnityLogger.Log("[EnemyAiTurnService] Pending AI action canceled while animation is playing.", LogCategory.AI);
            return;
        }

        ScheduleProcessingIfNeeded();
    }

    private void ProcessEnemyTurn()
    {
        if (_isProcessing)
            return;

        if (_animationGate.IsLocked)
        {
            UnityLogger.Log("[EnemyAiTurnService] AI turn blocked because animation gate is still locked.", LogCategory.AI);
            return;
        }

        if (_turnService.Mode != GameMode.SinglePlayer || _turnService.BattleState != BattleState.inProgress)
            return;

        _isProcessing = true;
        try
        {
            if (!ShouldControlCurrentUnit())
                return;

            var active = _turnService.ActiveObject as UnitModel;
            if (active == null)
            {
                UnityLogger.Log("[EnemyAiTurnService] Active AI object is null or not UnitModel. Ending turn.", LogCategory.AI);
                _turnService.EndTurn();
                return;
            }

            if (!TryBuildBestPlan(active, out var plan))
            {
                UnityLogger.Log($"[EnemyAiTurnService] No valid plan for {active.UnitType.Value} [{active.Team.Value}] at {active.Position.Value}", LogCategory.AI);
                if (_config.AutoEndTurnWhenNoPlan)
                {
                    _turnService.EndTurn();
                }
                return;
            }

            UnityLogger.Log($"[EnemyAiTurnService] Executing {plan.ActionType} from {plan.Context.FromCell} to {plan.Context.TargetCell} (attackFrom {plan.Context.AttackFromCell})", LogCategory.AI);
            var executed = _pipeline.Execute(plan.ActionType, plan.Context);
            if (!executed && ReferenceEquals(_turnService.ActiveObject, active))
            {
                UnityLogger.Log($"[EnemyAiTurnService] Pipeline declined {plan.ActionType}. Ending turn for {active.UnitType.Value}.", LogCategory.AI);
                _turnService.EndTurn();
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private bool ShouldControlCurrentUnit()
    {
        var active = _turnService.ActiveObject;
        return active != null &&
               _turnService.BattleState == BattleState.inProgress &&
               _battleControlModes.IsAiControlled(active.Team);
    }

    private bool TryBuildBestPlan(UnitModel activeUnit, out ActionPlan plan)
    {
        plan = ActionPlan.None;
        if (activeUnit == null || activeUnit.ModifiedStats == null)
            return false;

        var enemies = GetEnemiesOrderedByDistance(activeUnit);
        if (enemies.Count == 0)
            return false;

        if (_config.TryDirectAttackFirst)
        {
            foreach (var enemy in enemies)
            {
                if (TryBuildAttackPlan(activeUnit, enemy, out plan))
                    return true;
            }
        }

        if (_config.TryMoveTowardsEnemy)
        {
            foreach (var enemy in enemies)
            {
                if (TryBuildMovePlan(activeUnit, enemy, out plan))
                    return true;
            }
        }

        return false;
    }

    private List<UnitModel> GetEnemiesOrderedByDistance(UnitModel activeUnit)
    {
        return _turnService.CombatUnits
            .OfType<UnitModel>()
            .Where(unit => unit != null &&
                           unit != activeUnit &&
                           unit.Amount.Value > 0 &&
                           unit.Team.Value != activeUnit.Team.Value)
            .OrderBy(unit => _config.PreferNearestEnemy ? EstimateDistance(activeUnit.Position.Value, unit.Position.Value) : 0f)
            .ToList();
    }

    private float EstimateDistance(Vector2Int from, Vector2Int to)
    {
        if (_movementSystem.GetRouteIgnoringObstacles(from, to, out var route) && route != null && route.Count > 0)
            return _movementSystem.GetRouteCost(route);

        return float.MaxValue;
    }

    private bool TryBuildAttackPlan(UnitModel attacker, UnitModel enemy, out ActionPlan plan)
    {
        plan = ActionPlan.None;
        var attackerCell = attacker.Position.Value;
        var targetCell = enemy.Position.Value;

        var directContext = new ActionContext(attackerCell, targetCell, SpellType.None, attackerCell);
        if (_resolver.TryResolvePlan(directContext, out var directPlan) &&
            (directPlan.ActionType == ActionType.Attack || directPlan.ActionType == ActionType.RangedAttack))
        {
            plan = directPlan;
            return true;
        }

        if (!_config.TryMoveThenAttack)
            return false;

        var reachable = _movementSystem.GetReachableCells(attackerCell, attacker.ModifiedStats.MoveSpeed);
        ActionPlan bestPlan = ActionPlan.None;
        var bestCost = float.MaxValue;

        foreach (var cell in reachable)
        {
            if (cell != attackerCell)
            {
                var gridCell = _gameModel.GetCell(cell);
                if (gridCell == null || !gridCell.IsEmpty)
                    continue;
            }

            var context = new ActionContext(attackerCell, targetCell, SpellType.None, cell);
            if (!_resolver.TryResolvePlan(context, out var candidate) || candidate.ActionType != ActionType.MoveThenAttack)
                continue;

            var moveCost = GetMoveCost(attacker, cell);
            if (moveCost < bestCost)
            {
                bestCost = moveCost;
                bestPlan = candidate;
            }
        }

        if (bestPlan.ActionType != ActionType.None)
        {
            plan = bestPlan;
            return true;
        }

        return false;
    }

    private bool TryBuildMovePlan(UnitModel attacker, UnitModel enemy, out ActionPlan plan)
    {
        plan = ActionPlan.None;
        var attackerCell = attacker.Position.Value;
        var targetCell = enemy.Position.Value;

        if (!_movementSystem.GetRouteIgnoringObstacles(attackerCell, targetCell, out var route) || route == null || route.Count == 0)
            return false;

        var accessible = _movementSystem.GetAccessibleRoutePoints(route, attacker.ModifiedStats.MoveSpeed);
        if (accessible.Count <= 1)
            return false;

        for (var i = accessible.Count - 1; i >= 1; i--)
        {
            var candidateCell = accessible[i];
            var cell = _gameModel.GetCell(candidateCell);
            if (cell == null || !cell.IsEmpty)
                continue;

            var context = new ActionContext(attackerCell, candidateCell, SpellType.None, attackerCell);
            if (_resolver.TryResolvePlan(context, out var movePlan) && movePlan.ActionType == ActionType.Move)
            {
                plan = movePlan;
                return true;
            }
        }

        return false;
    }

    private float GetMoveCost(UnitModel attacker, Vector2Int targetCell)
    {
        var hasRoute = attacker.ModifiedStats.CanFly
            ? _movementSystem.GetRouteIgnoringObstacles(attacker.Position.Value, targetCell, out var route)
            : _movementSystem.GetRoute(attacker.Position.Value, targetCell, out route);

        if (!hasRoute || route == null || route.Count == 0)
            return float.MaxValue;

        return _movementSystem.GetRouteCost(route);
    }

    private void TryRandomizeDeployment()
    {
        if (_deploymentApplied || !_config.RandomizeEnemyDeployment || _turnService.Mode != GameMode.SinglePlayer)
            return;

        var cells = _gameModel.GetAllCells();
        if (cells == null)
            return;

        var allUnits = _turnService.CombatUnits.OfType<UnitModel>().ToList();
        if (allUnits.Count == 0)
            return;

        var width = 0;
        var height = 0;
        foreach (var cell in cells)
        {
            if (cell == null)
                continue;

            width = Mathf.Max(width, cell.X + 1);
            height = Mathf.Max(height, cell.Y + 1);
        }

        if (width <= 0 || height <= 0)
            return;

        var rows = Mathf.Clamp(2, 1, height);
        var rng = new System.Random();
        var aiTeams = allUnits
            .Select(unit => unit.Team.Value)
            .Where(team => team != _turnService.LocalTeam && _battleControlModes.IsAiControlled(team))
            .Distinct()
            .ToList();

        foreach (var team in aiTeams)
        {
            var availableCells = BuildDeploymentCells(team, width, height, rows)
                .Where(cell =>
                {
                    var gridCell = _gameModel.GetCell(cell);
                    return gridCell != null && (gridCell.IsEmpty || (gridCell.Unit != null && gridCell.Unit.Team.Value == team));
                })
                .OrderBy(_ => rng.Next())
                .ToList();

            var teamUnits = allUnits.Where(unit => unit.Team.Value == team).OrderBy(_ => rng.Next()).ToList();
            for (var i = 0; i < teamUnits.Count && i < availableCells.Count; i++)
            {
                var unit = teamUnits[i];
                var targetCell = availableCells[i];
                if (unit.Position.Value == targetCell)
                    continue;

                UnityLogger.Log($"[EnemyAiTurnService] Random deployment: {unit.UnitType.Value} [{team}] -> {targetCell}", LogCategory.AI);
                _gameModel.MoveObject(unit, targetCell);
            }
        }

        _deploymentApplied = true;
    }

    private static IEnumerable<Vector2Int> BuildDeploymentCells(Team team, int width, int height, int rows)
    {
        if (team == Team.Red)
        {
            for (var y = height - rows; y < height; y++)
            for (var x = 0; x < width; x++)
                yield return new Vector2Int(x, y);
        }
        else
        {
            for (var y = 0; y < rows; y++)
            for (var x = 0; x < width; x++)
                yield return new Vector2Int(x, y);
        }
    }
}
