using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class MoveActionHandlerTests
    {
        private GameModel _model;
        private MoveActionHandler _handler;
        [SetUp]
        public void SetUp()
        {
            _model = new GameModel(new UnitModelFactory(), new MovementSystem());
            _model.InitializeGrid(3, 3);
            var unit = new UnitModel(new(), UnitType.Archer, 0, 0, 3, true);
            _model.GetCell(new Vector2Int(0, 0)).AddContent(unit);
            _handler = new MoveActionHandler(new MovementSystem(), _model);
        }

        [Test]
        public void CanExecute_ValidMove_ReturnsTrue()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 0), default, new Vector2Int(0, 0));
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_NoUnit_ReturnsFalse()
        {
            var ctx = new ActionContext(new Vector2Int(2, 2), new Vector2Int(1, 0), default, new Vector2Int(0, 0));
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsAccessibleRoute()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 0), default, new Vector2Int(0, 0));
            var preview = _handler.GetPreview(ctx);
            Assert.That(preview.ToDictionary(), Is.Not.Empty);
        }
    }
}