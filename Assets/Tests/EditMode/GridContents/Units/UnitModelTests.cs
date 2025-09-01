using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UniRx;
using Tests.EditMode.GridContents.Units;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitModelTests
    {
        private UnitStats _baseStats;
        private UnitModel _unitModel;
        private const int TestX = 5;
        private const int TestY = 3;
        private const int TestAmount = 10;
        private const bool TestIsPlayer = true;

        [SetUp]
        public void SetUp()
        {
            // Создаем базовые характеристики для тестов
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
            _unitModel = new UnitModel(_baseStats, UnitType.Archer, TestX, TestY, TestAmount, TestIsPlayer);
        }

        [TearDown]
        public void TearDown()
        {
            if (_baseStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_baseStats);
            }
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Assert
            Assert.That(_unitModel.Position.Value, Is.EqualTo(new Vector2Int(TestX, TestY)));
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(TestAmount));
            Assert.That(_unitModel.IsBlueTeam.Value, Is.EqualTo(TestIsPlayer));
            Assert.That(_unitModel.UnitType.Value, Is.EqualTo(UnitType.Archer));
            Assert.That(_unitModel.CanAct.Value, Is.True);
            Assert.That(_unitModel.CanMove.Value, Is.True);
            Assert.That(_unitModel.GridContentType, Is.EqualTo(GridContentType.unit));
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesModifiedStats()
        {
            // Assert
            Assert.That(_unitModel.ModifiedStats, Is.Not.Null);
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_baseStats.Health));
            Assert.That(_unitModel.ModifiedStats.MaxHealth, Is.EqualTo(_baseStats.MaxHealth));
            Assert.That(_unitModel.ModifiedStats.Damage, Is.EqualTo(_baseStats.Damage));
        }

        [Test]
        public void Position_Setter_UpdatesPositionCorrectly()
        {
            // Arrange
            var newPosition = new Vector2Int(10, 15);

            // Act
            _unitModel.Position.Value = newPosition;

            // Assert
            Assert.That(_unitModel.Position.Value, Is.EqualTo(newPosition));
            Assert.That(_unitModel.X, Is.EqualTo(newPosition.x));
            Assert.That(_unitModel.Y, Is.EqualTo(newPosition.y));
        }

        [Test]
        public void MoveByRoute_WithValidRoute_UpdatesPositionAndInvokesEvent()
        {
            // Arrange
            var route = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(2, 2), new Vector2Int(3, 3) };
            var movedEventInvoked = false;
            _unitModel.Moved += (movedRoute) => 
            {
                movedEventInvoked = true;
                Assert.That(movedRoute, Is.EqualTo(route));
            };

            // Act
            _unitModel.MoveByRoute(route);

            // Assert
            Assert.That(_unitModel.Position.Value, Is.EqualTo(new Vector2Int(3, 3)));
            Assert.That(movedEventInvoked, Is.True);
        }

        [Test]
        public void SendDamage_WithValidAttackContext_ReturnsDamageContext()
        {
            // Arrange
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);

            // Act
            var damageContext = _unitModel.SendDamage(attackContext);

            // Assert
            Assert.That(damageContext, Is.Not.Null);
            Assert.That(damageContext.DamageAmount, Is.EqualTo(_baseStats.Damage * TestAmount));
            Assert.That(damageContext.Type, Is.EqualTo(DamageType.physical));
            Assert.That(damageContext.Source, Is.EqualTo(_unitModel));
        }

        [Test]
        public void SendDamage_WithValidAttackContext_InvokesAttackedEvent()
        {
            // Arrange
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);
            var attackedEventInvoked = false;
            _unitModel.Attacked += (damageContext) => attackedEventInvoked = true;

            // Act
            _unitModel.SendDamage(attackContext);

            // Assert
            Assert.That(attackedEventInvoked, Is.True);
        }

        [Test]
        public void SimulateSendDamage_WithValidAttackContext_ReturnsDamageContext()
        {
            // Arrange
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);

            // Act
            var damageContext = _unitModel.SimulateSendDamage(attackContext);

            // Assert
            Assert.That(damageContext, Is.Not.Null);
            Assert.That(damageContext.DamageAmount, Is.EqualTo(_baseStats.Damage * TestAmount));
            Assert.That(damageContext.Type, Is.EqualTo(DamageType.physical));
            Assert.That(damageContext.Source, Is.EqualTo(_unitModel));
        }

        [Test]
        public void RecieveDamage_WithDamageLessThanHealth_ReducesHealthCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var initialHealth = _unitModel.ModifiedStats.Health;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(initialHealth - 50));
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(TestAmount));
        }

        [Test]
        public void RecieveDamage_WithDamageEqualToHealth_KillsOneUnit()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1));
            Assert.That(damageContext.DieAmount, Is.EqualTo(1));
        }

        [Test]
        public void RecieveDamage_WithDamageGreaterThanHealth_KillsMultipleUnits()
        {
            // Arrange
            var damageContext = new DamageContext(250, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 2));
            Assert.That(damageContext.DieAmount, Is.EqualTo(2));
        }

        [Test]
        public void RecieveDamage_WithLethalDamage_InvokesDiedEvent()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            var diedEventInvoked = false;
            _unitModel.Died += () => diedEventInvoked = true;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(diedEventInvoked, Is.True);
        }

        [Test]
        public void RecieveDamage_WithAnyDamage_InvokesHittedEvent()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var hittedEventInvoked = false;
            _unitModel.Hitted += (ctx) => hittedEventInvoked = true;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(hittedEventInvoked, Is.True);
        }

        [Test]
        public void RecieveDamage_WithAnyDamage_InvokesHealthChangedEvent()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var healthChangedEventInvoked = false;
            _unitModel.HealthChanged += () => healthChangedEventInvoked = true;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(healthChangedEventInvoked, Is.True);
        }

        [Test]
        public void TakeTurn_InvokesTurnStartedEvent()
        {
            // Arrange
            var turnStartedEventInvoked = false;
            _unitModel.TurnStarted += () => turnStartedEventInvoked = true;

            // Act
            _unitModel.TakeTurn();

            // Assert
            Assert.That(turnStartedEventInvoked, Is.True);
        }

        [Test]
        public void EndTurn_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _unitModel.EndTurn());
        }

        [Test]
        public void CanMoveThrough_ReturnsFalse()
        {
            // Act & Assert
            Assert.That(_unitModel.CanMoveThrough(), Is.False);
        }

        [Test]
        public void ToString_ReturnsUnitTypeString()
        {
            // Act
            var result = _unitModel.ToString();

            // Assert
            Assert.That(result, Is.EqualTo(UnitType.Archer.ToString()));
        }

        [Test]
        public void ApplyStatusEffect_WithNonInvulnerableEffect_AppliesEffect()
        {
            // Arrange
            var statusEffect = new MockStatusEffect(StatusEffectType.Armored);

            // Act
            _unitModel.ApplyEffect(statusEffect);

            // Assert
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(1));
        }

        [Test]
        public void ApplyStatusEffect_WithInvulnerableEffect_DoesNotApplyEffect()
        {
            // Arrange
            _unitModel.InvulnerableEffects.Add(StatusEffectType.Armored);
            var statusEffect = new MockStatusEffect(StatusEffectType.Armored);

            // Act
            _unitModel.ApplyEffect(statusEffect);

            // Assert
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveEffect_RemovesEffectCorrectly()
        {
            // Arrange
            var statusEffect = new MockStatusEffect(StatusEffectType.Armored);
            _unitModel.ApplyEffect(statusEffect);
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(1));

            // Act
            _unitModel.RemoveEffect(statusEffect);

            // Assert
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(0));
        }

        [Test]
        public void RecieveDamage_WithDamageLessThanHealth_DoesNotTriggerDeath()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var deathEventInvoked = false;
            _unitModel.Died += () => deathEventInvoked = true;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(deathEventInvoked, Is.False, "Death event should not be invoked for non-lethal damage");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(TestAmount), "Unit amount should remain the same");
        }

        [Test]
        public void RecieveDamage_WithDamageEqualToHealth_TriggersDeath()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);
            var deathEventInvoked = false;
            _unitModel.Died += () => deathEventInvoked = true;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(deathEventInvoked, Is.True, "Death event should be invoked for lethal damage");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0), "Unit amount should be 0");
        }

        [Test]
        public void RecieveDamage_WithPartialStackDeath_DoesNotTriggerDeath()
        {
            // Arrange
            var damageContext = new DamageContext(150, DamageType.physical, null); // Урон больше здоровья одного юнита
            var deathEventInvoked = false;
            _unitModel.Died += () => deathEventInvoked = true;
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(deathEventInvoked, Is.False, "Death event should not be invoked for partial stack death");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), "Unit amount should decrease by 1");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_unitModel.BaseUnitStats.MaxHealth), "Remaining unit should have full health");
        }

        [Test]
        public void RecieveDamage_WithExactHealthKill_TriggersDeath()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null); // Точно здоровье одного юнита
            var deathEventInvoked = false;
            _unitModel.Died += () => deathEventInvoked = true;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(deathEventInvoked, Is.True, "Death event should be invoked for exact health kill");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0), "Unit amount should be 0");
        }

        [Test]
        public void RecieveDamage_WithOverkill_TriggersDeath()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null); // Урон намного больше здоровья
            var deathEventInvoked = false;
            _unitModel.Died += () => deathEventInvoked = true;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(deathEventInvoked, Is.True, "Death event should be invoked for overkill damage");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0), "Unit amount should be 0");
        }

        [Test]
        public void RecieveDamage_WithPartialDamage_UpdatesHealthCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var initialHealth = _unitModel.ModifiedStats.Health;
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(initialHealth - 50), 
                "Health should be reduced by damage amount");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount), 
                "Unit amount should remain the same for non-lethal damage");
        }

        [Test]
        public void RecieveDamage_WithExactHealthDamage_KillsOneUnit()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1 for exact health damage");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_unitModel.BaseUnitStats.MaxHealth), 
                "Next unit in stack should have full health");
            Assert.That(damageContext.DieAmount, Is.EqualTo(1), 
                "DieAmount should be 1 for one unit killed");
        }

        [Test]
        public void RecieveDamage_WithOverkillDamage_KillsMultipleUnits()
        {
            // Arrange
            var damageContext = new DamageContext(250, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act - Убиваем 2 юнита (100 + 100 = 200) и повреждаем третьего на 50
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 2), 
                "Unit amount should decrease by 2 for overkill damage");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(50), 
                "Remaining unit should have 50 health (100 - 50)");
            Assert.That(damageContext.DieAmount, Is.EqualTo(2), 
                "DieAmount should be 2 for two units killed");
        }

        [Test]
        public void RecieveDamage_WithMassiveDamage_KillsAllUnits()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0), 
                "Unit amount should be 0 for massive damage");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(0), 
                "Unit health should be 0 when all units are dead");
            Assert.That(damageContext.DieAmount, Is.EqualTo(initialAmount), 
                "DieAmount should equal initial amount for complete destruction");
        }

        [Test]
        public void RecieveDamage_WithZeroDamage_DoesNothing()
        {
            // Arrange
            var damageContext = new DamageContext(0, DamageType.physical, null);
            var initialHealth = _unitModel.ModifiedStats.Health;
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(initialHealth), 
                "Health should remain unchanged for zero damage");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount), 
                "Unit amount should remain unchanged for zero damage");
            Assert.That(damageContext.DieAmount, Is.EqualTo(0), 
                "DieAmount should be 0 for zero damage");
        }

        [Test]
        public void RecieveDamage_WithNegativeDamage_ThrowsException()
        {
            // Arrange
            var damageContext = new DamageContext(-50, DamageType.physical, null);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _unitModel.RecieveDamage(damageContext));
            Assert.That(exception.Message, Does.Contain("Damage amount cannot be negative"), 
                "Exception message should mention negative damage");
        }

        [Test]
        public void RecieveDamage_WithStatusEffects_AppliesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var initialHealth = _unitModel.ModifiedStats.Health;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(initialHealth - 50), 
                "Health should be reduced correctly with status effects");
            // Дополнительные проверки статус-эффектов можно добавить здесь
        }

        [Test]
        public void RecieveDamage_WithMultipleSmallDamages_AccumulatesCorrectly()
        {
            // Arrange
            var damage1 = new DamageContext(30, DamageType.physical, null);
            var damage2 = new DamageContext(40, DamageType.physical, null);
            var damage3 = new DamageContext(50, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act - Наносим несколько небольших уронов
            _unitModel.RecieveDamage(damage1); // 100 - 30 = 70
            _unitModel.RecieveDamage(damage2); // 70 - 40 = 30
            _unitModel.RecieveDamage(damage3); // 30 - 50 = -20, убиваем юнита

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1 after accumulated damage");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(80), 
                "Next unit should have 80 health (100 - 20)");
        }

        // Mock классы для тестирования
        private class MockDamagable : IDamagable
        {
            public bool IsBlueTeam => true;
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;

            public void RecieveDamage(DamageContext ctx) { }
            public void SimulateRecieveDamage(DamageContext ctx) { }
        }

        private class MockStatusEffect : StatusEffect
        {
            public MockStatusEffect(StatusEffectType type) : base(null, null, null) { }
        }
    }
}
