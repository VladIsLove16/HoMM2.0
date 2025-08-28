using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniRx;

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
            var statusEffect = new MockStatusEffect(StatusEffectType.Poison);

            // Act
            _unitModel.ApplyStatusEffect(statusEffect);

            // Assert
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(1));
        }

        [Test]
        public void ApplyStatusEffect_WithInvulnerableEffect_DoesNotApplyEffect()
        {
            // Arrange
            _unitModel.InvulnerableEffects.Add(StatusEffectType.Poison);
            var statusEffect = new MockStatusEffect(StatusEffectType.Poison);

            // Act
            _unitModel.ApplyStatusEffect(statusEffect);

            // Assert
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveEffect_RemovesEffectCorrectly()
        {
            // Arrange
            var statusEffect = new MockStatusEffect(StatusEffectType.Poison);
            _unitModel.ApplyStatusEffect(statusEffect);
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(1));

            // Act
            _unitModel.RemoveEffect(statusEffect);

            // Assert
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(0));
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
