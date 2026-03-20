using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Combat
{
    [TestFixture]
    public class EnemyAiTurnService_EditModeTests
    {
        private MovementSystem _movementSystem;
        private GameModel _gameModel;
        private TurnService _turnService;
        private ActionResolver _resolver;
        private ActionPipeline _pipeline;
        private EnemyAiTurnService _enemyAi;
        private UnitStats _baseStats;

        [SetUp]
        public void SetUp()
        {
            _movementSystem = new MovementSystem();
            _baseStats = CreateStats(health: 100, damage: 20, moveSpeed: 2, attackRange: 1, allowAdjacentRanged: true);

            var provider = new InlineStatsProvider();
            provider.Set(UnitType.Archer, _baseStats);
            provider.Set(UnitType.Witch, _baseStats);

            _gameModel = new GameModel(new UnitModelFactory(provider), _movementSystem);
            _turnService = new TurnService(new TurnQueue(), GameMode.SinglePlayer, Team.Blue);
            _resolver = new ActionResolver(_gameModel, _movementSystem);
            _pipeline = new ActionPipeline(_resolver, _turnService);
            _enemyAi = new EnemyAiTurnService(_turnService, _resolver, _pipeline, _movementSystem, _gameModel);
        }

        [TearDown]
        public void TearDown()
        {
            _enemyAi?.Dispose();
            if (_baseStats != null)
            {
                Object.DestroyImmediate(_baseStats);
            }
        }

        [Test]
        public void EnemyAi_AttacksAdjacentEnemy_OfSameUnitType()
        {
            _gameModel.InitializeGrid(3, 1);
            var blue = SpawnUnit(0, 0, UnitType.Archer, Team.Blue);
            var red = SpawnUnit(1, 0, UnitType.Archer, Team.Red);
            _turnService.AddCombatUnit(blue);
            _turnService.AddCombatUnit(red);

            _turnService.RunBattle();
            Assert.That(_turnService.ActiveObject, Is.SameAs(blue));

            _turnService.EndTurn();

            Assert.That(blue.ModifiedStats.Health, Is.EqualTo(80), "Красный AI должен атаковать соседнего синего даже при одинаковом UnitType.");
            Assert.That(_turnService.ActiveObject, Is.SameAs(blue), "После хода AI очередь должна вернуться к локальной команде.");
            Assert.That(red.Position.Value, Is.EqualTo(new Vector2Int(1, 0)));
        }

        [Test]
        public void EnemyAi_MovesTowardsNearestEnemy_WhenAttackIsNotAvailable()
        {
            _gameModel.InitializeGrid(5, 1);
            var blue = SpawnUnit(0, 0, UnitType.Archer, Team.Blue);
            var red = SpawnUnit(4, 0, UnitType.Witch, Team.Red);
            red.ModifiedStats.MoveSpeed = 2;
            red.ModifiedStats.AttackRange = 1;

            _turnService.AddCombatUnit(blue);
            _turnService.AddCombatUnit(red);

            _turnService.RunBattle();
            _turnService.EndTurn();

            Assert.That(red.Position.Value, Is.EqualTo(new Vector2Int(2, 0)), "AI должен подбежать к ближайшему врагу на максимальную доступную дистанцию.");
            Assert.That(_turnService.ActiveObject, Is.SameAs(blue));
        }

        [Test]
        public void EnemyAi_UsesLocalTeamLogic_NotHardcodedRed()
        {
            _gameModel.InitializeGrid(3, 1);
            var blue = SpawnUnit(0, 0, UnitType.Archer, Team.Blue);
            var green = SpawnUnit(1, 0, UnitType.Witch, Team.Green);
            _turnService.AddCombatUnit(blue);
            _turnService.AddCombatUnit(green);

            _turnService.RunBattle();
            Assert.That(_turnService.IsMyTurn, Is.True);

            _turnService.EndTurn();

            Assert.That(blue.ModifiedStats.Health, Is.EqualTo(80), "AI должен управлять любой non-local командой, а не только красной.");
            Assert.That(_turnService.IsMyTurn, Is.True, "После завершения хода non-local AI управление должно вернуться локальной команде.");
        }

        [Test]
        public void TurnService_IsMyTurn_IsBasedOnLocalTeam()
        {
            var turnService = new TurnService(new TurnQueue(), GameMode.SinglePlayer, Team.Blue);
            var blue = new CombatStub(Team.Blue);
            var red = new CombatStub(Team.Red);
            turnService.AddCombatUnit(blue);
            turnService.AddCombatUnit(red);

            turnService.RunBattle();
            Assert.That(turnService.ActiveObject, Is.SameAs(blue));
            Assert.That(turnService.IsMyTurn, Is.True);

            turnService.EndTurn();

            Assert.That(turnService.ActiveObject, Is.SameAs(red));
            Assert.That(turnService.IsMyTurn, Is.False, "В singleplayer мой ход должен определяться локальной командой, а не самим фактом singleplayer-режима.");
        }

        private UnitModel SpawnUnit(int x, int y, UnitType type, Team team)
        {
            var result = _gameModel.SpawnUnit(new UnitSpawnParams(x, y, type, 1, team));
            Assert.That(result.IsSuccess, Is.True, result.Message);
            return _gameModel.GetCell(new Vector2Int(x, y)).Unit;
        }

        private static UnitStats CreateStats(int health, int damage, int moveSpeed, int attackRange, bool allowAdjacentRanged)
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = health;
            stats.MaxHealth = health;
            stats.Damage = damage;
            stats.MoveSpeed = moveSpeed;
            stats.AttackRange = attackRange;
            stats.AllowAdjacentRanged = allowAdjacentRanged;
            stats.InvulnerableEffects = new List<StatusEffectType>();
            return stats;
        }

        private sealed class InlineStatsProvider : IUnitStatsProvider
        {
            private readonly Dictionary<UnitType, UnitStats> _stats = new();

            public IEnumerable<UnitType> Types => _stats.Keys;

            public bool TryGetBaseStats(UnitType type, out UnitStats stats)
            {
                return _stats.TryGetValue(type, out stats);
            }

            public void Set(UnitType type, UnitStats stats)
            {
                _stats[type] = stats;
            }
        }

        private sealed class CombatStub : ICombatObject
        {
            public CombatStub(Team team)
            {
                Team = team;
                Position = Vector2Int.zero;
                Stats = ScriptableObject.CreateInstance<UnitStats>();
                Stats.Health = 100;
                Stats.MaxHealth = 100;
                Stats.Damage = 10;
            }

            public Team Team { get; }
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;
            public void TakeTurn() { }
            public void EndTurn() { }
            public UnitType UnitType => UnitType.Archer;
            public UnitStats Stats { get; }
            public Action<ICombatObject> Died { get; set; }
        }
    }
}
