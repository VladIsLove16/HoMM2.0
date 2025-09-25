using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class MoveThenAttackHandlerTests
    {
        private GameModel _model;
        private MoveThenAttackHandler _handler;

        private UnitStats _baseStats;
        private UnitModel _unitModel;
        private const int TestX = 5;
        private const int TestY = 3;
        private const int TestAmount = 10;
        private const bool TestIsPlayer = true;
        [SetUp]
        public void SetUp()
        {
            _baseStats = ScriptableObject.CreateInstance<UnitStats>();
            _baseStats.Health = 100;
            _baseStats.MaxHealth = 100;
            _baseStats.Damage = 25;
            _baseStats.SpellPower = 15;
            _baseStats.Offense = 20;
            _baseStats.Defense = 15;
            _baseStats.MoveSpeed = 3;
            _baseStats.AttackRange = 2;
            _baseStats.CanFly = false;
            _baseStats.InvulnerableEffects = new List<StatusEffectType>();

            // Создаем модель юнита
            _model = new GameModel(new UnitModelFactory(), new MovementSystem());
            _model.InitializeGrid(3, 3);
            var attacker =  new UnitModel(_baseStats, UnitType.Archer, TestX, TestY, TestAmount, TestIsPlayer);
            var target = new UnitModel(_baseStats, UnitType.Archer, TestX + 1, TestY, TestAmount, TestIsPlayer);
            _model.GetCell(new Vector2Int(0, 0)).AddContent(attacker);
            _model.GetCell(new Vector2Int(1, 1)).AddContent(target);
            _handler = new MoveThenAttackHandler(new MovementSystem(), _model);
        }

        [Test]
        public void CanExecute_ValidMoveAndAttack_ReturnsTrue()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), default, new Vector2Int(1, 1));
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_NoTarget_ReturnsFalse()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(2, 2), default, new Vector2Int(2, 2));
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsAttackCell()
        {
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), default, new Vector2Int(1, 1));
            var preview = _handler.GetPreview(ctx);
            Assert.That(preview.ToDictionary()[CellState.hovered], Contains.Item(new Vector2Int(1, 1)));
        }
    }
}