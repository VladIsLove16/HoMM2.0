using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Actions
{
    [TestFixture]
    public sealed class ActionResolver_MeleeRange_EditModeTests
    {
        private MovementSystem _movementSystem;

        [SetUp]
        public void SetUp()
        {
            _movementSystem = new MovementSystem();
        }

        [Test]
        public void TryResolvePlan_ReturnsAttack_ForAdjacentMeleeUnitWithZeroAttackRange()
        {
            var resolver = CreateResolver(attackerRange: 0, attackerPosition: new Vector2Int(0, 0), targetPosition: new Vector2Int(1, 0));
            var context = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 0), SpellType.None, new Vector2Int(0, 0));

            var resolved = resolver.TryResolvePlan(context, out var plan);

            Assert.That(resolved, Is.True);
            Assert.That(plan.ActionType, Is.EqualTo(ActionType.Attack));
        }

        [Test]
        public void TryResolvePlan_ReturnsMoveThenAttack_ForMeleeUnitThatCanReachAdjacentCell()
        {
            var resolver = CreateResolver(attackerRange: 0, attackerPosition: new Vector2Int(0, 0), targetPosition: new Vector2Int(2, 0));
            var context = new ActionContext(new Vector2Int(0, 0), new Vector2Int(2, 0), SpellType.None, new Vector2Int(1, 0));

            var resolved = resolver.TryResolvePlan(context, out var plan);

            Assert.That(resolved, Is.True);
            Assert.That(plan.ActionType, Is.EqualTo(ActionType.MoveThenAttack));
        }

        [Test]
        public void TryResolvePlan_ReturnsMoveThenAttack_ForDiagonalAdjacentAttackCell()
        {
            var resolver = CreateResolver(attackerRange: 0, attackerPosition: new Vector2Int(0, 0), targetPosition: new Vector2Int(2, 2));
            var context = new ActionContext(new Vector2Int(0, 0), new Vector2Int(2, 2), SpellType.None, new Vector2Int(1, 1));

            var resolved = resolver.TryResolvePlan(context, out var plan);

            Assert.That(resolved, Is.True);
            Assert.That(plan.ActionType, Is.EqualTo(ActionType.MoveThenAttack));
        }

        [Test]
        public void TryResolvePlan_ReturnsRangedAttack_ForUnitWithPositiveAttackRange()
        {
            var resolver = CreateResolver(attackerRange: 3, attackerPosition: new Vector2Int(0, 0), targetPosition: new Vector2Int(2, 0));
            var context = new ActionContext(new Vector2Int(0, 0), new Vector2Int(2, 0), SpellType.None, new Vector2Int(0, 0));

            var resolved = resolver.TryResolvePlan(context, out var plan);

            Assert.That(resolved, Is.True);
            Assert.That(plan.ActionType, Is.EqualTo(ActionType.RangedAttack));
        }

        private ActionResolver CreateResolver(int attackerRange, Vector2Int attackerPosition, Vector2Int targetPosition)
        {
            var unitStatsProvider = new ResolverTestStatsProvider();
            unitStatsProvider.SetData(UnitType.Archer, CreateStats(attackerRange));
            unitStatsProvider.SetData(UnitType.Witch, CreateStats(0));

            var factory = new UnitModelFactory(unitStatsProvider);
            var gameModel = new GameModel(factory, _movementSystem);
            gameModel.InitializeGrid(5, 3);
            gameModel.SpawnUnit(new UnitSpawnParams(attackerPosition.x, attackerPosition.y, UnitType.Archer, 1, Team.Blue));
            gameModel.SpawnUnit(new UnitSpawnParams(targetPosition.x, targetPosition.y, UnitType.Witch, 1, Team.Red));
            return new ActionResolver(gameModel, _movementSystem);
        }

        private static UnitStats CreateStats(int attackRange)
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 10;
            stats.MaxHealth = 10;
            stats.Damage = 5;
            stats.MoveSpeed = 3;
            stats.AttackRange = attackRange;
            stats.AllowAdjacentRanged = true;
            stats.InvulnerableEffects = new List<StatusEffectType>();
            return stats;
        }

        private sealed class ResolverTestStatsProvider : IUnitStatsProvider
        {
            private readonly Dictionary<UnitType, UnitStats> _stats = new();

            public IEnumerable<UnitType> Types => _stats.Keys;

            public bool TryGetBaseStats(UnitType type, out UnitStats stats)
            {
                return _stats.TryGetValue(type, out stats);
            }

            public void SetData(UnitType type, UnitStats stats)
            {
                _stats[type] = stats;
            }
        }
    }
}
