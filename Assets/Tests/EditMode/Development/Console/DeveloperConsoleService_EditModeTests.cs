using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DeveloperConsoleService_EditModeTests
{
    private sealed class StubCommand : IDeveloperConsoleCommand
    {
        public string Key => "echo";
        public string Description => "Echoes arguments";
        public string Usage => "echo <message>";
        public IReadOnlyList<string> Aliases { get; } = new[] { "say", "print" };

        public List<(DeveloperConsoleCommandContext context, IReadOnlyList<string> args)> Invocations { get; } = new();

        public void Execute(DeveloperConsoleCommandContext context, IReadOnlyList<string> args, IDeveloperConsoleOutput output)
        {
            Invocations.Add((context, args));
            output.AppendLine(string.Join(' ', args));
        }
    }

    private sealed class NullCommandExecutor : IGameCommandExecutor
    {
        public ActionType? LastType { get; private set; }
        public ActionContext LastContext { get; private set; }

        public void Execute(ActionType type, ActionContext ctx)
        {
            LastType = type;
            LastContext = ctx;
        }

        public void StartBattle()
        {
        }
    }

    [Test]
    public void Submit_UsesRegisteredCommandAndProvidesContext()
    {
        var harness = new GameModelBuilder();
        var stubCommand = new StubCommand();
        var service = new DeveloperConsoleService(
            new[] { stubCommand },
            harness.GameModel,
            harness.GameViewModel,
            harness.CommandExecutor,
            harness.TurnService,
            harness.ActionResolver);

        service.Submit("echo hello world");

        Assert.That(stubCommand.Invocations, Has.Count.EqualTo(1));
        var invocation = stubCommand.Invocations[0];
        Assert.That(invocation.context.GameViewModel, Is.SameAs(harness.GameViewModel));
        Assert.That(invocation.args, Is.EqualTo(new[] { "hello", "world" }));
        Assert.That(service.Log, Has.Some.Matches<DeveloperConsoleLogEntry>(entry => entry.Message.Contains("hello world")));
    }

    [Test]
    public void Submit_ResolvesAliases()
    {
        var harness = new GameModelBuilder();
        var stubCommand = new StubCommand();
        var service = new DeveloperConsoleService(
            new[] { stubCommand },
            harness.GameModel,
            harness.GameViewModel,
            harness.CommandExecutor,
            harness.TurnService,
            harness.ActionResolver);

        service.Submit("say hi");

        Assert.That(stubCommand.Invocations, Has.Count.EqualTo(1));
        Assert.That(stubCommand.Invocations[0].args, Is.EqualTo(new[] { "hi" }));
    }

    [Test]
    public void Submit_UnknownCommand_AppendsError()
    {
        var harness = new GameModelBuilder();
        var service = new DeveloperConsoleService(
            new IDeveloperConsoleCommand[0],
            harness.GameModel,
            harness.GameViewModel,
            harness.CommandExecutor,
            harness.TurnService,
            harness.ActionResolver);

        service.Submit("unknown");

        Assert.That(service.Log[^1].Message, Does.Contain("Unknown command"));
        Assert.That(service.Log[^1].Type, Is.EqualTo(DeveloperConsoleLogType.Error));
    }

    private sealed class GameModelBuilder
    {
        public GameModel GameModel { get; }
        public GameViewModel GameViewModel { get; }
        public NullCommandExecutor CommandExecutor { get; } = new();
        public TurnSystem TurnSystem { get; } = new();
        public ITurnService TurnService { get; }
        public MovementSystem MovementSystem { get; } = new();
        public ActionResolver ActionResolver { get; }

        public GameModelBuilder()
        {
            var unitStats = ScriptableObject.CreateInstance<UnitStats>();
            unitStats.MaxHealth = 10;
            unitStats.Health = 10;
            unitStats.MoveSpeed = 5;
            unitStats.AttackRange = 1;
            unitStats.Damage = 3;

            var unitDefinition = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            unitDefinition.Stats = unitStats;
            unitDefinition.UnitType = UnitType.Archer;
            unitDefinition.name = "Unit_Def";

            var dataMap = new Dictionary<UnitType, UnitDefinitionSO>
            {
                { UnitType.Archer, unitDefinition }
            };

            var unitFactory = new UnitModelFactory(dataMap);
            GameModel = new GameModel(unitFactory, MovementSystem);
            ActionResolver = new ActionResolver(GameModel, MovementSystem);
            TurnService = new TurnService(TurnSystem);
            GameViewModel = new GameViewModel(GameModel, MovementSystem, CommandExecutor, TurnSystem, ActionResolver);
        }
    }
}
