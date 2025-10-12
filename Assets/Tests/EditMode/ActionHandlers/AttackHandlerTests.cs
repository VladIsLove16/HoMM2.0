using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.ActionHandlers
{
    [TestFixture]
    public class AttackHandlerTests
    {
        private GameModel _model;
        private AttackActionHandler _handler;

        private UnitStats _baseStats;
        private UnitModel _unitModel;
        private UnitModelFactory _modelFactory ;

        private const int attackerX = 0;
        private const int attackerY = 0;
        private const int damageableX = 0;
        private const int damageableY = 1;
        private const int teammateX = 1;
        private const int teammateY = 0;
        private const int TestAmount = 10;
        private ActionContext ctx = new(new Vector2Int(attackerX, attackerY), new Vector2Int(damageableX, damageableY), default, default);
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
            _baseStats.AttackRange = 1;
            _baseStats.CanFly = false;
            _baseStats.InvulnerableEffects = new List<StatusEffectType>();

            var moveSys = new MovementSystem();
            _modelFactory = new(new Dictionary<UnitType, UnitDefinitionSO>
            {
                { UnitType.Archer, new UnitDefinitionSO { Stats = _baseStats } }
            });
            _model = new GameModel(_modelFactory, moveSys);
            _model.InitializeGrid(3, 3);
            _model.SpawnUnit(new UnitSpawnParams(attackerX, attackerY, UnitType.Archer, TestAmount, Team.Blue));
            _model.SpawnUnit(new UnitSpawnParams(damageableX, damageableY, UnitType.Archer, TestAmount, Team.Red));
            _handler = new AttackActionHandler(moveSys, _model);
        }
        
        [Test]
        public void CanExecute_DiagonalAttackWith1AttackRange_ReturnsTrue()
        {
            _model.SpawnUnit(new UnitSpawnParams(1, 1, UnitType.Archer, TestAmount, Team.Red));
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 1), default, default);
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_Attack_ReturnsTrue()
        {
            var ctx = new ActionContext(new Vector2Int(attackerX, attackerY), new Vector2Int(damageableX, damageableY), default, default);         
            Assert.IsTrue(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_NoTarget_ReturnFalse()
        {
             ActionContext ctx = new(new Vector2Int(attackerX, attackerY), new Vector2Int(1, 1), default, default);
             Assert.IsFalse(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_AttackSelfUnit_ReturnFalse()
        {
            var ctx = new ActionContext(new Vector2Int(attackerX, attackerY), new Vector2Int(attackerX, attackerY), default, default);
            Assert.False(_handler.CanExecute(ctx));
        }

        [Test]
        public void CanExecute_AttackTeammate_ReturnFalse()
        {
            _model.SpawnUnit(new UnitSpawnParams(teammateX, teammateY, UnitType.Archer, TestAmount, Team.Blue));
            var ctx = new ActionContext(new Vector2Int(attackerX, attackerY), new Vector2Int(teammateX, teammateY), default, default);
            Assert.False(_handler.CanExecute(ctx));
        }

        [Test]
        public void GetPreview_ReturnsAttackCell()
        {
            var preview = _handler.GetPreview(ctx);
            Assert.That(preview.ToDictionary()[CellState.attackTarget], Contains.Item(new Vector2Int(damageableX, damageableY)));
        }
    }
}