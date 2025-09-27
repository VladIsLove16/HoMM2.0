using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UniRx;
using UnityEditor;

namespace Tests.EditMode.GridContents.Input
{
    [TestFixture]
    public class Input_MoveIntegrationTest
    {
        private GameModel _model;
        private MovementSystem _movement;
        private UnitModelFactory _unitModelFactory;

        [SetUp]
        public void SetUp()
        {
            var dataMap = TestDataFactory.CreateSingleUnitData(UnitType.Archer, moveSpeed:3);
            _unitModelFactory = new UnitModelFactory(dataMap);
            _movement = new MovementSystem();
            _model = new GameModel(_unitModelFactory, _movement);

            _model.InitializeGrid(5, 5);

            var spawn = new UnitSpawnParams(1, 1, UnitType.Archer, 1, Team.Blue);
            Assert.True(_model.SpawnUnit(spawn).IsSuccess);
        }

        [Test]
        public void Move_By_CommandRoute_UpdatesModelAndTriggersEvent()
        {
            UnitModel unit = ((GameCell)_model.GetAllCells()[1, 1]).Unit;
            Assert.NotNull(unit);

            bool movedRaised = false;
            unit.MovedByRoute += (route) => { movedRaised = true; };

            var route = new List<Vector2Int> { new Vector2Int(2, 1), new Vector2Int(3, 1) };

            // Имитируем прямой вызов серверной части gateway через сервис (в тесте без сети)
            // В данном простом тесте напрямую обновим модель, как это делает сервер.
            _model.MoveObject(unit, route);


            Assert.AreEqual(new Vector2Int(3, 1), unit.Position.Value);
            Assert.True(movedRaised);
        }
    }
}