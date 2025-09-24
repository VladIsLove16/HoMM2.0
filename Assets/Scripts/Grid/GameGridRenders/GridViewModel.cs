using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

public class GridViewModel : IGridViewModel
{
     private TurnSystem _turnSystem;
    private MovementSystem _movementSystem;
    private GameModel _gameModel;
    private IWorldToCellProvider worldToCellProvider;
    public Action<Dictionary<CellState, List<Vector2Int>>> PreviewChanged { get; set; }
    public GridViewModel(TurnSystem turnSystem, MovementSystem movementSystem, GameViewModel gameViewModel)
    {
        _turnSystem = turnSystem;
        _movementSystem = movementSystem;
        _turnSystem.ActiveObject.Subscribe(ActiveObjectChanged);
        _gameViewModel = gameViewModel;
        _gameViewModel.CellHovered += HandleCellHovered;
    }
    private void ActiveObjectChanged(ICombatObject combatObject)
    {
        if (combatObject == null)
            return;
        var reachableCells = _movementSystem.GetReachableCells(combatObject.Position, combatObject.Stats.MoveSpeed);
        var dict = new Dictionary<CellState, List<Vector2Int>>();
        dict.Add(CellState.reachableCell,new(reachableCells));

        PreviewChanged.Invoke(dict);
    }

    public void HandleCellHovered(Vector2Int coords)
    {
        var dict = new Dictionary<CellState, List<Vector2Int>>();

        UnitModel unitModel = _gameModel.GetCell(coords).Unit;
        IDamageSource damageSource = _turnSystem.ActiveObject as IDamageSource;

        if (_turnSystem.IsMyTurn)
        {
            if (unitModel != null)
            {
                //melee attack
                if (_gameModel.CanRangeAttack(damageSource, unitModel))
                    dict.Add(CellState.attackTarget, new() { coords });
                //move thenattack
                else if (_gameModel.CanMoveThenAttack(damageSource, unitModel))
                {
                    _movementSystem.GetRoute(_turnSystem.ActiveObject.Value.Position, coords, out var route);
                    dict.Add(CellState.attackTarget, new() { coords });
                    dict.Add(CellState.accessibleRoutePoint, new(route));
                }
            }
            else
            {
                //move
                dict.Add(CellState.hovered, new() { coords });
                if (unitModel.CanMove.Value)
                {
                    var reachableCells = _movementSystem.GetReachableCells(_turnSystem.ActiveObject.Value.Position, _turnSystem.ActiveObject.Value.Stats.MoveSpeed);
                    var accessibleRoutePoints = _movementSystem.GetAccessibleRoutePoints(reachableCells, _turnSystem.ActiveObject.Value.Stats.MoveSpeed);
                    var inaccessibleRoutePoints = reachableCells;
                    foreach (var cell in accessibleRoutePoints)
                    {
                        inaccessibleRoutePoints.Remove(cell);
                    }
                    dict.Add(CellState.accessibleRoutePoint, accessibleRoutePoints);
                    dict.Add(CellState.inaccessibleRoutePoint, inaccessibleRoutePoints);
                }
            }
        }
        if (unitModel != null)
        {
            var reachableEnemyCells = _movementSystem.GetReachableCells(unitModel.Position.Value, unitModel.ModifiedStats.MoveSpeed);
            dict.Add(CellState.enemyReachableCell, new(reachableEnemyCells));
        }
        PreviewChanged.Invoke(dict);
    }
    public void HandleCellClicked(Vector2Int coords)
    {
        UnitModel unitModel = _gameModel.GetCell(coords).Unit;
        IDamageSource damageSource = _turnSystem.ActiveObject as IDamageSource;
        if (_turnSystem.IsMyTurn)
        {
            if (unitModel != null)
            {
                //melee attack
                if (_gameModel.CanRangeAttack(damageSource, unitModel))
                    _gameModel.ExecuteAttackAction(damageSource, coords);
                //move thenattack
                else if (_gameModel.CanMoveThenAttack(damageSource, unitModel))
                {
                    _movementSystem.GetRoute(_turnSystem.ActiveObject.Value.Position, coords, out var route);
                    _gameModel.ExecuteMoveThenAttackAction(unitModel, route, coords);
                }
            }
            else
            {
                if (unitModel.CanMove.Value)
                {
                    _movementSystem.GetRoute(_turnSystem.ActiveObject.Value.Position, coords, out var route);
                    _gameModel.ExecuteMoveAction(unitModel, route);
                }
            }
        }
    }
}