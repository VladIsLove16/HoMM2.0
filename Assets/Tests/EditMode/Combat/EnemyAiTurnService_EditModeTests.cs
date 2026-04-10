using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests.EditMode.Combat
{
    [TestFixture]
    public class EnemyAiTurnService_EditModeTests
    {
        private MovementSystem _movementSystem;
        private GameModel _gameModel;
        private TurnService _turnService;
        private BattleControlModeService _battleControlModes;
        private BattleAnimationGate _animationGate;
        private ActionResolver _resolver;
        private ActionPipeline _pipeline;
        private EnemyAiTurnService _enemyAi;
        private AnimationSpeedSettings _animationSpeedSettings;
        private BattleAiControlConfigSO _config;
        private UnitStats _baseStats;

        [SetUp]
        public void SetUp()
        {
            _movementSystem = new MovementSystem();
            _animationSpeedSettings = ScriptableObject.CreateInstance<AnimationSpeedSettings>();
            _config = ScriptableObject.CreateInstance<BattleAiControlConfigSO>();
            SetPrivateField(_config, "aiTurnDelaySeconds", 0f);
            SetPrivateField(_config, "randomizeEnemyDeployment", false);
            _baseStats = CreateStats(health: 100, damage: 20, moveSpeed: 2, attackRange: 1, allowAdjacentRanged: true);

            var provider = new InlineStatsProvider();
            provider.Set(UnitType.Archer, _baseStats);
            provider.Set(UnitType.Witch, _baseStats);

            _gameModel = new GameModel(new UnitModelFactory(provider), _movementSystem);
            _turnService = new TurnService(new TurnQueue(), GameMode.SinglePlayer, Team.Blue);
            _animationGate = new BattleAnimationGate();
            _resolver = new ActionResolver(_gameModel, _movementSystem);
            _pipeline = new ActionPipeline(_resolver, _turnService);
            _battleControlModes = new BattleControlModeService(_turnService, _animationSpeedSettings, _config);
            _enemyAi = new EnemyAiTurnService(_turnService, _battleControlModes, _animationGate, _resolver, _pipeline, _movementSystem, _gameModel, _config);
        }

        [TearDown]
        public void TearDown()
        {
            _enemyAi?.Dispose();
            _battleControlModes?.Dispose();

            if (_animationSpeedSettings != null)
            {
                Object.DestroyImmediate(_animationSpeedSettings);
            }

            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }

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

            Assert.That(blue.ModifiedStats.Health, Is.EqualTo(80));
            Assert.That(_turnService.ActiveObject, Is.SameAs(blue));
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

            Assert.That(red.Position.Value, Is.EqualTo(new Vector2Int(2, 0)));
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

            Assert.That(blue.ModifiedStats.Health, Is.EqualTo(80));
            Assert.That(_turnService.IsMyTurn, Is.True);
        }

        [Test]
        public void EnemyAi_DoesNotProcess_WhenEnemyTeamSetToManual()
        {
            _gameModel.InitializeGrid(3, 1);
            var blue = SpawnUnit(0, 0, UnitType.Archer, Team.Blue);
            var red = SpawnUnit(1, 0, UnitType.Archer, Team.Red);
            _turnService.AddCombatUnit(blue);
            _turnService.AddCombatUnit(red);
            _battleControlModes.SetEnemyTeamsMode(BattleControlMode.Manual);

            _turnService.RunBattle();
            _turnService.EndTurn();

            Assert.That(_turnService.ActiveObject, Is.SameAs(red));
            Assert.That(blue.ModifiedStats.Health, Is.EqualTo(100));
        }

        [Test]
        public void EnemyAi_CanControl_LocalTeam_WhenSwitchedToAi()
        {
            _gameModel.InitializeGrid(3, 1);
            var blue = SpawnUnit(0, 0, UnitType.Archer, Team.Blue);
            var red = SpawnUnit(1, 0, UnitType.Archer, Team.Red);
            _turnService.AddCombatUnit(blue);
            _turnService.AddCombatUnit(red);
            _battleControlModes.SetLocalTeamMode(BattleControlMode.AI);
            _battleControlModes.SetEnemyTeamsMode(BattleControlMode.Manual);

            _turnService.RunBattle();

            Assert.That(red.ModifiedStats.Health, Is.EqualTo(80));
            Assert.That(_turnService.ActiveObject, Is.SameAs(red));
        }

        [Test]
        public void BattleControlModes_EnableFastResolve_SetsAiAndAnimationOverride()
        {
            _gameModel.InitializeGrid(2, 1);
            var blue = SpawnUnit(0, 0, UnitType.Archer, Team.Blue);
            var red = SpawnUnit(1, 0, UnitType.Archer, Team.Red);
            _turnService.AddCombatUnit(blue);
            _turnService.AddCombatUnit(red);

            _battleControlModes.EnableFastResolve();

            Assert.That(_battleControlModes.IsFastResolveActive.Value, Is.True);
            Assert.That(_battleControlModes.IsAiControlled(Team.Blue), Is.True);
            Assert.That(_battleControlModes.IsAiControlled(Team.Red), Is.True);
            Assert.That(_animationSpeedSettings.PlaybackMultiplier, Is.EqualTo(10f));
            Assert.That(_animationSpeedSettings.IsInstant, Is.False);
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
            Assert.That(turnService.IsMyTurn, Is.False);
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

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Field '{fieldName}' not found on {target.GetType().Name}");
            field.SetValue(target, value);
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
            System.Action<ICombatObject> ICombatObject.Died { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        }
    }
}
