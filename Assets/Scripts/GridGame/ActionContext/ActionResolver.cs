using System;
using System.Collections.Generic;
using UnityEngine;
public class ActionResolver
{
    public event Action<ActionPlan> ActionResolved;
    public System.Action ActionNotResolved;

    private readonly MoveThenAttackHandler MoveThenAttackHandler;
    private MoveActionHandler MoveActionHandler;
    private RangedAttackHandler RangedAttackHandler;
    private AttackActionHandler AttackActionHandler;
    private SpellActionHandler SpellActionHandler;
    private readonly MovementSystem _movementSystem;
    private readonly GameModel _gameModel;

    private Dictionary<ActionType, IActionHandler> actionDict = new();
    public ActionResolver(GameModel gameModel, MovementSystem movementSystem)
    {
        _movementSystem = movementSystem;
        _gameModel = gameModel;

        MoveThenAttackHandler = new(movementSystem, gameModel);
        MoveActionHandler = new(movementSystem, gameModel);
        RangedAttackHandler = new(movementSystem, gameModel);
        AttackActionHandler = new(movementSystem, gameModel);
        SpellActionHandler = new(movementSystem, gameModel);
        actionDict.Add(ActionType.MoveThenAttack, MoveThenAttackHandler);
        actionDict.Add(ActionType.Attack, AttackActionHandler);
        actionDict.Add(ActionType.Move, MoveActionHandler);
        actionDict.Add(ActionType.RangedAttack, RangedAttackHandler);
        actionDict.Add(ActionType.Spell, SpellActionHandler);
    }
    public IActionHandler Resolve(ActionType type, ActionContext actionContext)
    {
        return actionDict[type];
    }
    public bool TryResolvePlan(ActionContext ctx, out ActionPlan plan)
    {
        if (ResolveFlat(ctx, out var actionType))
        {
            plan = new ActionPlan(actionType, ctx);
            ActionResolved?.Invoke(plan);
            return true;
        }

        ActionNotResolved?.Invoke();
        plan = ActionPlan.None;
        return false;
    }

    /// <summary>
    /// Монолитная проверка всех действий в одном месте.
    /// Возвращает true/false и выбранный ActionType.
    /// </summary>
    public bool ResolveFlat(ActionContext ctx, out ActionType actionType)
    {

        var fromCell = _gameModel.GetCell(ctx.FromCell);
        var attackFromCell = _gameModel.GetCell(ctx.AttackFromCell);
        var targetCell = _gameModel.GetCell(ctx.TargetCell);

        var fromUnit = fromCell?.Unit;
        var attackFromUnit = attackFromCell?.Unit;
        var targetUnit = targetCell?.Unit;
        bool targetOccupied = targetUnit != null;
        bool attackCellOccupiedByOther = attackFromCell != fromCell && attackFromCell != null && attackFromUnit != null;

        // 1) Спелл, если выбран
        if (ctx.AbilityUsed != SpellType.None)
        {
            actionType = ActionType.Spell;
            return true;
        }

        // 2) Нет цели в TargetCell — пробуем Move, но только если клетка пустая
        if (!targetOccupied)
        {
            if (targetCell != null && targetCell.IsEmpty && TryMove(fromUnit as IMoveable, ctx.TargetCell))
            {
                actionType = ActionType.Move;
                return true;
            }
            actionType = ActionType.None;
            return false;
        }

        // 3) Если цель союзная — перемещаться нельзя (клетка занята)
        if (fromUnit != null && targetUnit.Team.Value == fromUnit.Team.Value)
        {
            actionType = ActionType.None;
            return false;
        }

        // 4) Вражеская цель: сначала пробуем стрелять, затем удар с подходом/без
        var attackerForRanged = fromUnit;
        if (attackerForRanged != null && attackerForRanged.ModifiedStats != null && IsEnemy(attackerForRanged, targetUnit))
        {
            _movementSystem.GetRouteIgnoringObstacles(ctx.FromCell, ctx.TargetCell, out var routeRanged);
            var distRanged = _movementSystem.GetRouteCost(routeRanged);
            bool isAdjacent = distRanged <= 1.01f;
            bool allowAdjacentRanged = attackerForRanged.ModifiedStats.AllowAdjacentRanged;

            if (distRanged > 0 && distRanged <= attackerForRanged.ModifiedStats.AttackRange &&
                (allowAdjacentRanged || !isAdjacent))
            {
                actionType = ActionType.RangedAttack;
                return true;
            }
        }

        // 5) Move-then-attack (если нужно переместиться в attackFromCell)
        if (fromUnit is IMoveable mover &&
            attackFromCell != null &&
            ctx.AttackFromCell != ctx.FromCell &&
            !attackCellOccupiedByOther &&
            IsMeleeUnit(fromUnit) &&
            IsEnemy(fromUnit, targetUnit))
        {
            if (TryMoveToCellThenMelee(mover, ctx.AttackFromCell, fromUnit, ctx.TargetCell))
            {
                actionType = ActionType.MoveThenAttack;
                return true;
            }
        }

        // 6) Ближний бой из текущей клетки
        if (fromUnit != null && IsEnemy(fromUnit, targetUnit))
        {
            _movementSystem.GetRouteIgnoringObstacles(ctx.FromCell, ctx.TargetCell, out var routeMelee);
            var distMelee = _movementSystem.GetRouteCost(routeMelee);
            if (routeMelee != null && fromUnit.ModifiedStats != null && Mathf.Floor(distMelee) <= fromUnit.ModifiedStats.AttackRange)
            {
                actionType = ActionType.Attack;
                return true;
            }
        }

        // 7) Ничего не подошло
        actionType = ActionType.None;
        return false;

        bool TryMove(IMoveable mover, Vector2Int target)
        {
            if (mover == null) return false;
            if (mover.CanFly ? !_movementSystem.GetRouteIgnoringObstacles(mover.Position, target, out var routeFly)
                             : !_movementSystem.GetRoute(mover.Position, target, out routeFly))
                return false;
            var accessible = _movementSystem.GetAccessibleRoutePoints(routeFly, mover.MoveSpeed);
            if (accessible.Count == 0 || _movementSystem.GetRouteCost(accessible) > mover.MoveSpeed)
                return false;
            var targetCellObj = _gameModel.GetCell(target);
            if (targetCellObj != null && !targetCellObj.IsEmpty)
                return false;
            return accessible[^1] == target;
        }

        bool TryMoveToCellThenMelee(IMoveable mover, Vector2Int attackFrom, UnitModel attacker, Vector2Int target)
        {
            if (mover == null) return false;
            if (attacker == null) return false;
            var attackCellObj = _gameModel.GetCell(attackFrom);
            if (attackCellObj != null && !attackCellObj.IsEmpty && attackFrom != ctx.FromCell)
                return false;

            List<Vector2Int> routeTo;
            var hasRouteTo = mover.CanFly
                ? _movementSystem.GetRouteIgnoringObstacles(mover.Position, attackFrom, out routeTo)
                : _movementSystem.GetRoute(mover.Position, attackFrom, out routeTo);
            if (!hasRouteTo) return false;

            var accessible = _movementSystem.GetAccessibleRoutePoints(routeTo, mover.MoveSpeed);
            if (accessible.Count == 0 || _movementSystem.GetRouteCost(accessible) > mover.MoveSpeed)
                return false;
            if (accessible[^1] != attackFrom)
                return false;

            _movementSystem.GetRouteIgnoringObstacles(attackFrom, target, out var routeAttack);
            if (routeAttack == null) return false;

            var dist = _movementSystem.GetRouteCost(routeAttack);
            if (attacker.ModifiedStats == null || Mathf.Floor(dist) > attacker.ModifiedStats.AttackRange)
                return false;

            return true;
        }

        bool IsEnemy(UnitModel a, UnitModel b) => b != null && a != null && a.Team.Value != b.Team.Value;
        bool IsMeleeUnit(UnitModel unit) => unit != null && unit.ModifiedStats != null && unit.ModifiedStats.AttackRange <= 1;
    }
}
