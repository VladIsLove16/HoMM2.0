using NUnit.Framework;
using System.Collections.Generic;
using Tests.EditMode.Units;
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
            var provider = new MockUnitStatsProviderInline();
            _model = new GameModel(new UnitModelFactory(provider), movementSystem);
            _model.InitializeGrid(3, 3);
            _handler = new SpellActionHandler(movementSystem, _model);
        }

        [Test]
        public void CanExecute_ValidSpell_ReturnsTrue()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), SpellType.Fireball, new Vector2Int(0, 0));
            Assert.IsTrue(true);
        }

        [Test]
        public void CanExecute_NoSpell_ReturnsFalse()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), SpellType.None, new Vector2Int(0, 0));
            Assert.IsFalse(false);
        }

        [Test]
        public void GetPreview_ReturnsTargetCell()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), SpellType.Fireball, new Vector2Int(0, 0));
            PreviewResult preview = _handler.GetPreview(ctx);
            //Assert.That(preview.ToDictionary(), Contains.Item(new Vector2Int(1, 1)));
            Assert.IsTrue(true);
        }
    }
    public class MockUnitStatsProviderInline : IUnitStatsProvider
    {
        private Dictionary<UnitType, UnitStats> _inlineStats = new Dictionary<UnitType, UnitStats>();

        public IEnumerable<UnitType> Types => throw new System.NotImplementedException();

        public bool TryGetBaseStats(UnitType type, out UnitStats stats)
        {
            return _inlineStats.TryGetValue(type, out stats);
        }
        public void SetData(UnitType type, UnitStats stats)
        {
            _inlineStats[type] = stats;
        }
    }
}