using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class RangedAttackHandlerTests
    {
        private GameModel _model;
        private RangedAttackHandler _handler;

        [SetUp]
        public void SetUp()
        {
            var ms = new MovementSystem();
            _model = new GameModel(new UnitModelFactory(), ms);
            _model.InitializeGrid(3, 3);
            var attacker = new UnitModel(new(), UnitType.Archer, 0, 0, 1, true);
            var target = new UnitModel(new(), UnitType.Archer, 0, 2, 1, true);
            _model.GetCell(new Vector2Int(0, 0)).AddContent(attacker);
            _model.GetCell(new Vector2Int(2, 2)).AddContent(target);
            _handler = new RangedAttackHandler(ms,_model);
        }

        [Test]
        public void CanExecute_ValidRangedAttack_ReturnsTrue()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(2, 2), default, new Vector2Int(0, 0));
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_OutOfRange_ReturnsFalse()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(10, 10), default, new Vector2Int(0, 0));
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsTargetCell()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(2, 2), default, new Vector2Int(0, 0));
            var preview = _handler.GetPreview(ctx);
            Assert.That(preview.ToDictionary()[CellState.hovered], Contains.Item(new Vector2Int(2, 2)));
        }
    }
}