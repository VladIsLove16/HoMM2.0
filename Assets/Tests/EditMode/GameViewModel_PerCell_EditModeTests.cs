using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Common;
using UnityEngine;

namespace Tests.EditMode.ViewModels
{
    [TestFixture]
    public sealed class GameViewModel_PerCell_EditModeTests
    {
        [Test]
        public void ActiveUnitChange_SetsReachableAndEnemyCellsWithinDeployment()
        {
            using var harness = new Harness(5, 5);
            PreviewResult preview = null;
            harness.ViewModel.PreviewChanged += result => preview = result;

            harness.StartBattle();

            Assert.That(preview, Is.Not.Null);
            var data = preview.ToDictionary();
            Assert.That(data.ContainsKey(CellState.reachableCell), Is.True);
            Assert.That(data[CellState.reachableCell], Is.Not.Empty);
            Assert.That(data.ContainsKey(CellState.enemyCell), Is.True);
            Assert.That(data[CellState.enemyCell], Does.Contain(harness.RedUnit.Position.Value));
        }

        [Test]
        public void HoverEmptyCell_ProducesRouteAndRouteEndStates()
        {
            using var harness = new Harness(5, 5);
            harness.StartBattle();

            PreviewResult updated = null;
            harness.ViewModel.PreviewUpdated += result => updated = result;

            var target = new Vector2Int(1, 0);
            harness.ViewModel.HandleCellHovered(target, -Vector2Int.one);

            Assert.That(updated, Is.Not.Null);
            var data = updated.ToDictionary();
            Assert.That(data.ContainsKey(CellState.accessibleRoutePoint), Is.True);
            Assert.That(data[CellState.accessibleRoutePoint], Is.Not.Empty);
            Assert.That(data[CellState.routeEndAccessible], Does.Contain(target));
            Assert.That(data[CellState.routeEndBlocked], Is.Empty);
        }

        [Test]
        public void HoverEnemyCell_AddsEnemyReachablePreview()
        {
            using var harness = new Harness(5, 5);
            harness.StartBattle();

            PreviewResult updated = null;
            harness.ViewModel.PreviewUpdated += result => updated = result;

            var target = harness.RedUnit.Position.Value;
            harness.ViewModel.HandleCellHovered(target, -Vector2Int.one);

            Assert.That(updated, Is.Not.Null);
            var data = updated.ToDictionary();
            Assert.That(data.ContainsKey(CellState.enemyReachableCell), Is.True);
            Assert.That(data[CellState.enemyReachableCell], Is.Not.Empty);
        }

        private sealed class Harness : IDisposable
        {
            private readonly TestStatsProvider _statsProvider = new();
            private readonly NullCommandExecutor _commandExecutor = new();

            public int Width { get; }
            public int Height { get; }
            public GameModel GameModel { get; }
            public MovementSystem MovementSystem { get; } = new();
            public ActionResolver ActionResolver { get; }
            public TurnService TurnService { get; } = new(new TurnQueue());
            public TurnStateViewModel TurnState { get; }
            public GameViewModel ViewModel { get; }
            public SinglePlayerStartConfigurationSO StartConfiguration { get; }
            public UnitModel BlueUnit { get; private set; }
            public UnitModel RedUnit { get; private set; }

            public Harness(int width, int height)
            {
                Width = width;
                Height = height;
                StartConfiguration = ScriptableObject.CreateInstance<SinglePlayerStartConfigurationSO>();
                StartConfiguration.SetTeam(Team.Blue);
                StartConfiguration.SetBattlefieldBottomTeam(Team.Blue);

                var stats = ScriptableObject.CreateInstance<UnitStats>();
                stats.MaxHealth = 10;
                stats.Health = 10;
                stats.MoveSpeed = 3;
                stats.AttackRange = 1;
                stats.AllowAdjacentRanged = true;
                _statsProvider.SetData(UnitType.Archer, stats);

                var factory = new UnitModelFactory(_statsProvider);
                GameModel = new GameModel(factory, MovementSystem);
                ActionResolver = new ActionResolver(GameModel, MovementSystem);
                TurnState = new TurnStateViewModel(TurnService);
                ViewModel = new GameViewModel(GameModel, MovementSystem, _commandExecutor, TurnState, ActionResolver, StartConfiguration, new TestGridRenderSettings());

                GameModel.InitializeGrid(Width, Height);
                GameModel.SpawnUnit(new UnitSpawnParams(0, 0, UnitType.Archer, 1, Team.Blue));
                GameModel.SpawnUnit(new UnitSpawnParams(Width - 1, Height - 1, UnitType.Archer, 1, Team.Red));

                BlueUnit = GameModel.GetCell(new Vector2Int(0, 0)).Unit as UnitModel;
                RedUnit = GameModel.GetCell(new Vector2Int(Width - 1, Height - 1)).Unit as UnitModel;
            }

            public void StartBattle()
            {
                TurnService.AddCombatUnit(BlueUnit);
                TurnService.AddCombatUnit(RedUnit);
                TurnService.RunBattle();
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
