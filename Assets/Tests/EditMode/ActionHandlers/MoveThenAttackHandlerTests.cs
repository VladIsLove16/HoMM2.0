using NUnit.Framework;
using System;
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
        private const int attackerX = 0;
        private const int attackerY = 0;
        private const int attackerCellX = 0;
        private const int attackerCellY = 1;
        private const int damageableX = 1;
        private const int damageableY = 1;
        private const int teammateCellX = 1;
        private const int teammateCellY = 0;
        private const int TestAmount = 10;
        private ActionContext ctx = new(new Vector2Int(attackerX, attackerY), new Vector2Int(damageableX, damageableY), default, new Vector2Int(attackerCellX, attackerCellY));
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

            var moveSys = new MovementSystem();
            var dict = new Dictionary<UnitType, UnitDefinitionSO>
            {
                { UnitType.Archer, new UnitDefinitionSO { Stats = _baseStats } }
            };
            var modelFactory = new UnitModelFactory(dict);
            _model = new GameModel(modelFactory, moveSys);
            _model.InitializeGrid(3, 3);
            _model.SpawnUnit(new UnitSpawnParams(attackerX, attackerY, UnitType.Archer, TestAmount, Team.Blue));
            _model.SpawnUnit(new UnitSpawnParams(damageableX, damageableY , UnitType.Archer, TestAmount, Team.Red));
            _model.SpawnUnit(new UnitSpawnParams(teammateCellX, teammateCellY, UnitType.Archer, TestAmount, Team.Blue));
            _handler = new MoveThenAttackHandler(moveSys, _model);
        }
        [Test]
        public void CanExecute_NoTarget_ReturnFalse()
        {
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_ValidMoveAndAttack_ReturnsTrue()
        {
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_InValidMoveButValidAttack_ReturnsFalse()
        {
             ActionContext ctx = new(new Vector2Int(attackerX, attackerY), new Vector2Int(damageableX, damageableY), default, new Vector2Int(damageableX, damageableY));
             Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_ValidMoveButInValidAttack_ReturnsFalse()
        {
            ActionContext ctx = new(new Vector2Int(attackerX, attackerY), new Vector2Int(attackerX, attackerX), default, new Vector2Int(attackerCellX, attackerCellY));
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsAttackCell()
        {
            var preview = _handler.GetPreview(ctx);
            Assert.That(preview.ToDictionary()[CellState.attackTarget], Contains.Item(new Vector2Int(damageableX, damageableY)));
        }
        [Test]
        public void CanExecute_AttackEmptyCell_ReturnsFalse()
        {
            var ctx = new ActionContext(new Vector2Int(attackerX, attackerY), new Vector2Int(damageableX, damageableY), default, new Vector2Int(attackerCellX, attackerCellY));
            Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_AttackTeammate_ReturnFalse()
        {
            var ctx = new ActionContext(new Vector2Int(attackerX, attackerY), new Vector2Int(teammateCellX, teammateCellY), default, new Vector2Int(attackerCellX, attackerCellY));
            Assert.False(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_AttackSelfUnit_ReturnFalse()
        {
            var ctx = new ActionContext(new Vector2Int(attackerX, attackerY), new Vector2Int(attackerX, attackerY), default, new Vector2Int(attackerCellX, attackerCellY));
            Assert.False(_handler.CanExecute(ctx));
        }
    }
}

namespace Tests.EditMode.ActionHandlers
{
}