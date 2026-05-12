using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Tests.TestHelpers;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.GameFlow
{
    [TestFixture]
    public class GameController_PlayModeTests
    {
        private readonly List<ScriptableObject> _createdAssets = new();
        private readonly List<GameObject> _createdObjects = new();

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            SceneTransitionTestSetup.DestroyService();
            foreach (var asset in _createdAssets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }
            _createdAssets.Clear();
            foreach (var go in _createdObjects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            _createdObjects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Start_WithProviderInitializesGridAndSpawnsUnits()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var entry = CreateEntry(4, 3, CreateContent(UnitType.Archer, 1, 1, Team.Blue));
            service.SetSelectedConfiguration(0);
            service.SetTeam(Team.Red);
            var turnService = new TurnService();
            var gameModel = CreateSpyGameModel();
            var controller = CreateController(turnService, service, gameModel, entry);

            controller.gameObject.SetActive(true);
            yield return null;

            Assert.That(gameModel.InitializeGridCalls, Is.EqualTo(1));
            Assert.That(gameModel.SpawnUnitCalls, Is.EqualTo(1));
            Assert.That(turnService.CombatUnits.Count, Is.EqualTo(1));
            Assert.That(turnService.CombatUnits[0].Team, Is.EqualTo(Team.Blue));
        }

        [UnityTest]
        public IEnumerator Start_UsesProviderTeamForTurnSystem()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var entry = CreateEntry(2, 2);
            service.SetSelectedConfiguration(0);
            service.SetTeam(Team.Red);
            var turnSystem = new TurnService();
            var controller = CreateController(turnSystem, service, CreateSpyGameModel(), entry);

            controller.gameObject.SetActive(true);
            yield return null;

            Assert.That(turnSystem.LocalTeam, Is.EqualTo(Team.Red));
        }

        [UnityTest]
        public IEnumerator Start_SetsBattleStateToReplacement()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var entry = CreateEntry(2, 2);
            service.SetSelectedConfiguration(0);
            var turnSystem = new TurnService();
            var controller = CreateController(turnSystem, service, CreateSpyGameModel(), entry);

            controller.gameObject.SetActive(true);
            yield return null;

            Assert.That(turnSystem.BattleState, Is.EqualTo(BattleState.replacement));
        }

        [UnityTest]
        public IEnumerator Setup_WithNullProviderUsesDefaultEntry()

        {
            var service = ScriptableObject.CreateInstance<SinglePlayerStartConfigurationSO>();
            var defaultEntry = CreateEntry(5, 4, CreateContent(UnitType.Archer, 0, 0, Team.Blue));
            var turnSystem = new TurnService();
            var gameModel = CreateSpyGameModel();
            var controller = CreateController(turnSystem, service, gameModel, defaultEntry);

            controller.gameObject.SetActive(true);
            yield return null;

            Assert.That(gameModel.LastGridSize.width, Is.EqualTo(5));
            Assert.That(gameModel.LastGridSize.height, Is.EqualTo(4));
            Assert.That(turnSystem.LocalTeam, Is.EqualTo(Team.Blue));
        }

        [UnityTest]
        public IEnumerator CreateGridContent_CanBeInvokedManuallyToAddUnits()
        {
            var service = SceneTransitionTestSetup.EnsureService();
            var initialEntry = CreateEntry(2, 2);
            service.SetSelectedConfiguration(0);
            var turnService = new TurnService();
            var gameModel = CreateSpyGameModel();
            var controller = CreateController(turnService, service, gameModel, initialEntry);

            controller.gameObject.SetActive(true);
            yield return null;

            var extraEntry = CreateEntry(2, 2, CreateContent(UnitType.Archer, 1, 0, Team.Red));
            controller.CreateGridContent(extraEntry);
            yield return null;

            Assert.That(gameModel.SpawnUnitCalls, Is.EqualTo(1));
            Assert.That(turnService.CombatUnits.Count, Is.EqualTo(1));
            Assert.That(turnService.CombatUnits[0].Team, Is.EqualTo(Team.Red));
        }

        private GameController CreateController(
            ITurnService turnSystem,
            SinglePlayerStartConfigurationSO startConfiguration,
            SpyGameModel gameModel,
            GridContentEntrySO defaultEntry)
        {
            var go = new GameObject("GameController");
            go.SetActive(false);
            _createdObjects.Add(go);
            var controller = go.AddComponent<GameController>();
            controller.Construct(gameModel, turnSystem);
            startConfiguration.SetAvailableConfigurations(new List<GridContentEntrySO> { defaultEntry });
            SetPrivateField(controller, "_startConfiguration", startConfiguration);
            return controller;
        }

        private SpyGameModel CreateSpyGameModel()
        {
            var stats = CreateStats();
            MockUnitStatsProviderInline unitStatsProvider = new MockUnitStatsProviderInline();
            unitStatsProvider.SetData(UnitType.Archer, stats);
            var factory = new UnitModelFactory(unitStatsProvider);
            var movement = new MovementSystem();
            return new SpyGameModel(factory, movement);
        }

        private UnitStats CreateStats()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 100;
            stats.MaxHealth = 100;
            stats.Damage = 10;
            stats.MoveSpeed = 3;
            stats.InvulnerableEffects = new List<StatusEffectType>();
            _createdAssets.Add(stats);
            return stats;
        }

        private GridContentEntrySO.UnitContent CreateContent(UnitType type, int x, int y, Team team)
        {
            return new GridContentEntrySO.UnitContent
            {
                unitType = type,
                Amount = 1,
                X = x,
                Y = y,
                Team = team
            };
        }

        private GridContentEntrySO CreateEntry(int width, int height, params GridContentEntrySO.UnitContent[] contents)
        {
            var entry = ScriptableObject.CreateInstance<GridContentEntrySO>();
            typeof(GridContentEntrySO).GetField("width", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(entry, width);
            typeof(GridContentEntrySO).GetField("height", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(entry, height);
            entry.contents = new List<GridContentEntrySO.UnitContent>(contents);
            _createdAssets.Add(entry);
            return entry;
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private class SpyGameModel : GameModel
        {
            public int InitializeGridCalls { get; private set; }
            public int SpawnUnitCalls { get; private set; }
            public (int width, int height) LastGridSize { get; private set; }

            public SpyGameModel(UnitModelFactory factory, MovementSystem movement) : base(factory, movement)
            {
            }

            public override void InitializeGrid(int width, int height)
            {
                base.InitializeGrid(width, height);
                InitializeGridCalls++;
                LastGridSize = (width, height);
            }

            public override OperationResult SpawnUnit(UnitSpawnParams spawnParams)
            {
                SpawnUnitCalls++;
                return base.SpawnUnit(spawnParams);
            }
        }
    }
    public class MockUnitStatsProviderInline : IUnitStatsProvider
    {
        private Dictionary<UnitType, UnitStats> _inlineStats = new Dictionary<UnitType, UnitStats>();

        public IEnumerable<UnitType> Types => throw new System.NotImplementedException();

        public bool TryGetBaseStats(UnitType type, out UnitStats stats)
        {
            return _inlineStats.TryGetValue(type, out stats);
        }
        public void SetData(UnitType type, UnitStats stats)
        {
            _inlineStats[type] = stats;
        }
    }
}





