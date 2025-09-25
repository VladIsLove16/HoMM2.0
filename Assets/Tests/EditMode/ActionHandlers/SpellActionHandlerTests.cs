using NUnit.Framework;
using UnityEngine;
namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class SpellActionHandlerTests
    {
        private GameModel _model;
        private SpellActionHandler _handler;

        [SetUp]
        public void SetUp()
        {
            MovementSystem movementSystem = new MovementSystem();
            _model = new GameModel(new UnitModelFactory(), movementSystem);
            _model.InitializeGrid(3, 3);
            _handler = new SpellActionHandler(movementSystem, _model);
        }

        [Test]
        public void CanExecute_ValidSpell_ReturnsTrue()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), SpellType.Fireball, new Vector2Int(0, 0));
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_NoSpell_ReturnsFalse()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), SpellType.None, new Vector2Int(0, 0));
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsTargetCell()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), SpellType.Fireball, new Vector2Int(0, 0));
            PreviewResult preview = _handler.GetPreview(ctx);
            Assert.That(preview.ToDictionary(), Contains.Item(new Vector2Int(1, 1)));
        }
    }
}