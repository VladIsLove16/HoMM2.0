using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Tests.TestHelpers;
using Tests.EditMode.ActionHandlers;
using Tests.Common;

public class ConsoleGameView_EditModeTests
{
    [SetUp]
    public void SetUp()
    {
        var service = SceneTransitionTestSetup.EnsureService();
        service.SetGameMode(GameMode.SinglePlayer);
        service.SetTeam(Team.Blue);
    }

    [TearDown]
    public void TearDown()
    {
        SceneTransitionTestSetup.DestroyService();
    }

    [Test]
    public void ConsoleGameView_ReactToGridInitializationAndSpawns()
    {
        var harness = new GameModelBuilder();
        var output = new TestConsoleOutput();
        var gridState = new ConsoleGridState();
        var consoleView = new ConsoleGameView(harness.GameViewModel, harness.TurnStateViewModel, gridState);
        consoleView.Initialize();

        harness.GameModel.InitializeGrid(3, 3);

        Assert.That(output.Messages, Has.Some.Contains("Grid initialized 3x3"));

        var spawnResult = harness.GameModel.SpawnUnit(new UnitSpawnParams(0, 0, UnitType.Archer, 1, Team.Blue));
        Assert.That(spawnResult.IsSuccess, Is.True);
        Assert.That(output.Messages, Has.Some.Contains("Unit spawned: Archer [Blue]"));

        var unit = harness.GameModel.GetUnits()[0];
        harness.TurnService.AddCombatUnit(unit);
        harness.TurnService.RunBattle();

        Assert.That(output.Messages, Has.Some.Contains("Active unit: Archer"));

        consoleView.Dispose();
        harness.TurnStateViewModel.Dispose();
    }

    [Test]
    public void ConsoleGridRenderer_BindsGridInitialization()
    {
        var gridState = new ConsoleGridState();
        var renderer = new ConsoleGridRenderer(gridState);
        var viewModel = new Tests.EditMode.Input.CellInputHandler_EditModeIntegrationTests.TestGridViewModel();

        renderer.Bind(viewModel);
        viewModel.SimulateGridInit(2, 2);

        var representation = gridState.BuildRepresentation();
        Assert.That(representation, Does.Contain("00"));

        renderer.Clear();
        renderer.Unbind(viewModel);
    }

    private sealed class TestConsoleOutput : IDeveloperConsoleOutput
    {
        public readonly List<string> Messages = new();
        public void AppendLine(string message) => Messages.Add(message);
        public void AppendWarning(string message) => Messages.Add($"WARN:{message}");
        public void AppendError(string message) => Messages.Add($"ERR:{message}");
    }

    private sealed class GameModelBuilder
    {
        public GameModel GameModel { get; }
        public GameViewModel GameViewModel { get; }
        public TurnService ConcreteTurnService { get; }
        public TurnStateViewModel TurnStateViewModel { get; }
        public ITurnService TurnService => ConcreteTurnService;
        public MovementSystem MovementSystem { get; } = new();
        public IGridRenderSettings GridSettings { get; } = new TestGridRenderSettings();
        public ActionResolver ActionResolver { get; }
        public IGameCommandExecutor CommandExecutor { get; } = new NullCommandExecutor();

        public GameModelBuilder()
        {
            var unitStats = ScriptableObject.CreateInstance<UnitStats>();
            unitStats.MaxHealth = 10;
            unitStats.Health = 10;
            unitStats.MoveSpeed = 5;
            unitStats.AttackRange = 1;
            unitStats.Damage = 3;

            var provider = new MockUnitStatsProviderInline();   
            provider.SetData(UnitType.Archer, unitStats);
            var unitFactory = new UnitModelFactory(unitStats);
            GameModel = new GameModel(unitFactory, MovementSystem);
            ActionResolver = new ActionResolver(GameModel, MovementSystem);
            ConcreteTurnService = new TurnService(new TurnQueue());
            TurnStateViewModel = new TurnStateViewModel(ConcreteTurnService);
            GameViewModel = new GameViewModel(GameModel, MovementSystem, CommandExecutor, TurnStateViewModel, ActionResolver, GridSettings);
        }
    }

    private sealed class NullCommandExecutor : IGameCommandExecutor
    {
        public bool Execute(ActionType type, ActionContext ctx)
        {
            return true;
        }

        public void StartBattle()
        {
        }
    }
}
