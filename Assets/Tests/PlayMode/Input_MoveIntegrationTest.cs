using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

public class Input_MoveIntegrationTest : ZenjectIntegrationTestFixture
{
    private GameModel _model;
    private MovementSystem _movement;
    private NetworkUnitCommandService _commandService;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        PreInstall();

        Container.Bind<MovementSystem>().AsSingle();
        Container.Bind<UnitModelFactory>().AsSingle();
        Container.Bind<GameModel>().AsSingle();
        Container.Bind<NetworkUnitCommandService>().AsSingle();

        PostInstall();

        _movement = Container.Resolve<MovementSystem>();
        _model = Container.Resolve<GameModel>();
        _commandService = Container.Resolve<NetworkUnitCommandService>();

        _model.InitializeGrid(5, 5);

        var unitStats = ScriptableObject.CreateInstance<UnitStats>();
        unitStats.MoveSpeed = 3;
        var spawn = new UnitSpawnParams(UnitType.Archer, 1, 1, 1, true);
        Assert.True(_model.SpawnUnit(spawn).Success);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Move_By_CommandRoute_UpdatesModelAndTriggersEvent()
    {
        UnitModel unit = ((GameCell)_model.GetAllCells()[1,1]).Unit;
        Assert.NotNull(unit);

        bool movedRaised = false;
        _model.UnitMovedByRoute += (content, route) => { if (content == unit) movedRaised = true; };

        var route = new List<Vector2Int> { new Vector2Int(2,1), new Vector2Int(3,1) };

        // Имитируем прямой вызов серверной части gateway через сервис (в тесте без сети)
        // В данном простом тесте напрямую обновим модель, как это делает сервер.
        _model.MoveObject(unit, route);

        yield return null;

        Assert.AreEqual(new Vector2Int(3,1), unit.Position.Value);
        Assert.True(movedRaised);
    }
}



