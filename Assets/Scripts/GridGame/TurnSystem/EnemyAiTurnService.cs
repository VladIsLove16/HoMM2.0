using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public sealed class EnemyAiTurnService : IDisposable
{
    private readonly ITurnService _turnService;
    private readonly ActionResolver _resolver;
    private readonly ActionPipeline _pipeline;
    private readonly MovementSystem _movementSystem;
    private readonly GameModel _gameModel;
    private readonly CompositeDisposable _disposables = new();

    private bool _isProcessing;

    public EnemyAiTurnService(
        ITurnService turnService,
        ActionResolver resolver,
        ActionPipeline pipeline,
        MovementSystem movementSystem,
        GameModel gameModel)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _movementSystem = movementSystem ?? throw new ArgumentNullException(nameof(movementSystem));
        _gameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));

        _turnService.ActiveObjectStream
            .Subscribe(_ => ProcessEnemyTurnsIfNeeded())
            .AddTo(_disposables);

        _turnService.BattleStateStream
            .Subscribe(_ => ProcessEnemyTurnsIfNeeded())
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    private void ProcessEnemyTurnsIfNeeded()
    {
        if (_isProcessing)
            return;

        if (_turnService.Mode != GameMode.SinglePlayer || _turnService.BattleState != BattleState.inProgress)
            return;

        _isProcessing = true;
        try
        {
            while (ShouldControlCurrentUnit())
            {
                var active = _turnService.ActiveObject as UnitModel;
                if (active == null)
                {
                    _turnService.EndTurn();
                    continue;
                }

                if (!TryBuildBestPlan(active, out var plan))
                {
                    UnityLogger.Log($"[EnemyAiTurnService] No valid plan for {active.UnitType.Value} [{active.Team.Value}] at {active.Position.Value}", LogCategory.AI);
                    _turnService.EndTurn();
                    continue;
                }

                var executed = _pipeline.Execute(plan.ActionType, plan.Context);
                if (!executed && ReferenceEquals(_turnService.ActiveObject, active))
                {
                    _turnService.EndTurn();
                }
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
               active.Team != _turnService.LocalTeam;
    }

    private bool TryBuildBestPlan(UnitModel activeUnit, out ActionPlan plan)
    {
        plan = ActionPlan.None;
        if (activeUnit == null || activeUnit.ModifiedStats == null)
            return false;

        var enemies = GetEnemiesOrderedByDistance(activeUnit);
        if (enemies.Count == 0)
            return false;

        foreach (var enemy in enemies)
        {
            if (TryBuildAttackPlan(activeUnit, enemy, out plan))
                return true;
        }

        foreach (var enemy in enemies)
        {
            if (TryBuildMovePlan(activeUnit, enemy, out plan))
                return true;
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
            .OrderBy(unit => EstimateDistance(activeUnit.Position.Value, unit.Position.Value))
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
}
