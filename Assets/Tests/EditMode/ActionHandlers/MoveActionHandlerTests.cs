using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class MoveActionHandlerTests
    {
        private GameModel _model;
        private MoveActionHandler _handler;
        private MovementSystem _moveSys;
        private UnitModelFactory _factory;
        private Vector2Int unitPos = new(0, 0);
        [SetUp]
        public void SetUp()
        {
            _moveSys = new MovementSystem();
            var UnitStats = ScriptableObject.CreateInstance<UnitStats>();
            UnitStats.MoveSpeed = 3;
            var statsProvider = new MockUnitStatsProviderInline();
            statsProvider.SetData(UnitType.Archer,UnitStats);
            _factory = new UnitModelFactory(statsProvider);
            _model = new GameModel(_factory, _moveSys);
            _model.InitializeGrid(3, 3);
            _model.SpawnUnit(new(unitPos.x, unitPos.y, UnitType.Archer, 1, Team.Blue));
            _handler = new MoveActionHandler(_moveSys, _model);
        }

        [Test]
        public void CanExecute_ValidMove_ReturnsTrue()
        {
            var ctx = new ActionContext(unitPos, new Vector2Int(1, 0), default, default);
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_NoUnit_ReturnFalse()
        {
            var ctx = new ActionContext(new Vector2Int(2, 2), new Vector2Int(1, 0), default,default);
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_NoPath_ReturnFalse()
        {
            var moveSys  = new StubMovementSystem();
            _model = new GameModel(_factory, moveSys);
            _model.InitializeGrid(3, 3);
            _model.SpawnUnit(new(unitPos.x, unitPos.y, UnitType.Archer, 1, Team.Blue));

            _handler = new MoveActionHandler(moveSys, _model);
            var ctx = new ActionContext(unitPos, new Vector2Int(1, 0), default,default);
            Assert.False(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsAccessibleRoute()
        {
            var ctx = new ActionContext(unitPos, new Vector2Int(1, 0), default, default);
            var preview = _handler.GetPreview(ctx);
            Assert.That(preview.ToDictionary(), Is.Not.Empty);
        }
    }
    public class StubMovementSystem : MovementSystem
    {
        public override bool GetRoute(Vector2Int from, Vector2Int to, out List<Vector2Int> route)
        {
            route = null;
            return false;
        }
    }
}