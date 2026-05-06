using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Common;
using UnityEngine;

namespace Tests.EditMode.ViewModels
{
    [TestFixture]
    public sealed class GameViewModel_ToGridPair_EditModeTests
    {
        [Test]
        public void ToGridPair_ReturnsRightNeighbor_WhenPointIsOnRightHalfOfCell()
        {
            using var harness = new Harness();

            var position = new Vector3(0.35f, 0f, 0.05f);
            var success = harness.ViewModel.ToGridPair(position, out var coords);

            Assert.That(success, Is.True);
            Assert.That(coords.Key, Is.EqualTo(Vector2Int.zero));
            Assert.That(coords.Value, Is.EqualTo(Vector2Int.right));
        }

        [Test]
        public void ToGridPair_ReturnsUpperNeighbor_WhenPointIsOnUpperHalfOfCell()
        {
            using var harness = new Harness();

            var position = new Vector3(0.05f, 0f, 0.35f);
            var success = harness.ViewModel.ToGridPair(position, out var coords);

            Assert.That(success, Is.True);
            Assert.That(coords.Key, Is.EqualTo(Vector2Int.zero));
            Assert.That(coords.Value, Is.EqualTo(Vector2Int.up));
        }

        [Test]
        public void ToGridPair_ReturnsUpperRightNeighbor_WhenPointIsNearCorner()
        {
            using var harness = new Harness();

            var position = new Vector3(0.4f, 0f, 0.4f);
            var success = harness.ViewModel.ToGridPair(position, out var coords);

            Assert.That(success, Is.True);
            Assert.That(coords.Key, Is.EqualTo(Vector2Int.zero));
            Assert.That(coords.Value, Is.EqualTo(Vector2Int.one));
        }

        private sealed class Harness : IDisposable
        {
            private readonly TestStatsProvider _statsProvider = new();
            private readonly NullCommandExecutor _commandExecutor = new();

            public GameModel GameModel { get; }
            public MovementSystem MovementSystem { get; } = new();
            public ActionResolver ActionResolver { get; }
            public TurnService TurnService { get; } = new(new TurnQueue());
            public TurnStateViewModel TurnState { get; }
            public GameViewModel ViewModel { get; }
            public GameConfigurationService GameConfigurationService { get; }

            public Harness()
            {
                GameConfigurationService = ScriptableObject.CreateInstance<GameConfigurationService>();
                GameConfigurationService.SetTeam(Team.Blue);
                GameConfigurationService.SetBattlefieldBottomTeam(Team.Blue);

                var stats = ScriptableObject.CreateInstance<UnitStats>();
                stats.MaxHealth = 10;
                stats.Health = 10;
                stats.MoveSpeed = 3;
                stats.AttackRange = 0;
                stats.InvulnerableEffects = new List<StatusEffectType>();
                _statsProvider.SetData(UnitType.Archer, stats);

                var renderSettings = new TestGridRenderSettings
                {
                    CellSize = 1f,
                    CellPadding = 0.1f
                };

                var factory = new UnitModelFactory(_statsProvider);
                GameModel = new GameModel(factory, MovementSystem);
                ActionResolver = new ActionResolver(GameModel, MovementSystem);
                TurnState = new TurnStateViewModel(TurnService);
                ViewModel = new GameViewModel(GameModel, MovementSystem, _commandExecutor, TurnState, ActionResolver, GameConfigurationService, renderSettings);

                GameModel.InitializeGrid(3, 3);
            }

            public void Dispose()
            {
                ViewModel.Dispose();
                TurnState.Dispose();
            }
        }

        private sealed class NullCommandExecutor : IGameCommandExecutor
        {
            public void StartBattle()
            {
            }

            public bool Execute(ActionType type, ActionContext ctx)
            {
                return true;
            }

            public bool TryDeployUnit(Vector2Int fromCell, Vector2Int toCell)
            {
                return true;
            }
        }

        private sealed class TestStatsProvider : IUnitStatsProvider
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
