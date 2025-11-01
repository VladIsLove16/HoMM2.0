using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class RangedAttackHandlerTests : ScriptableObject
    {
        private GameModel _model;
        private RangedAttackHandler _handler;
        private Vector2Int fromCell = new Vector2Int(0, 0);
        private Vector2Int toCell = new Vector2Int(2, 2);
        private Vector2Int teammateCell = new Vector2Int(1, 2);
        ActionContext validCtx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(2, 2), default, default);

        [SetUp]
        public void SetUp()
        {

            var ms = new MovementSystem();
            Dictionary<UnitType, UnitDefinitionSO> unitDatas = new Dictionary<UnitType, UnitDefinitionSO>();
            UnitDefinitionSO unitDefinitionSO = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            unitDefinitionSO.UnitStats = new();
            unitDefinitionSO.UnitStats.AttackRange = 3;
            unitDatas.Add(UnitType.Archer, unitDefinitionSO);
            var factory = new UnitModelFactory(unitDatas);
            _model = new GameModel(factory, ms);
            _model.InitializeGrid(5, 5);
            _model.SpawnUnit(new UnitSpawnParams(fromCell.x, fromCell.y, UnitType.Archer, 1, Team.Blue));
            _model.SpawnUnit(new UnitSpawnParams(toCell.x, toCell.y, UnitType.Archer, 1, Team.Red));
            _model.SpawnUnit(new UnitSpawnParams(teammateCell.x, teammateCell.y, UnitType.Archer, 1, Team.Blue));
            _handler = new RangedAttackHandler(ms,_model);
        }

        [Test]
        public void CanExecute_ValidRangedAttack_ReturnsTrue()
        {
            Assert.IsTrue(_handler.CanExecute(validCtx));
        }

        [Test]
        public void CanExecute_OutOfRange_ReturnsFalse()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(4, 4), default, default);
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_EmptyCell_ReturnsFalse()
        {
            var ctx = new ActionContext(fromCell, new Vector2Int(1, 1), default, default);
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_AttackSelfUnit_ReturnFalse()
        {
            var ctx = new ActionContext(fromCell, fromCell, default, default);
            Assert.False(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_AttackTeammate_ReturnFalse()
        {
            var ctx = new ActionContext(fromCell, teammateCell, default, default);
            Assert.False(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsTargetCell()
        {
            var preview = _handler.GetPreview(validCtx);
            Assert.That(preview.ToDictionary()[CellState.attackTarget], Contains.Item(toCell));
        }
    }
}