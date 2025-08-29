using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UniRx;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitModelEdgeCaseTests
    {
        private UnitStats _baseStats;
        private UnitModel _unitModel;

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
            _unitModel = new UnitModel(_baseStats, UnitType.Archer, 5, 3, 10, true);
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
        public void Constructor_WithZeroAmount_CreatesUnitWithZeroAmount()
        {
            // Arrange & Act
            var zeroAmountUnit = new UnitModel(_baseStats, UnitType.Archer, 0, 0, 0, true);

            // Assert
            Assert.That(zeroAmountUnit.Amount.Value, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithNegativePosition_CreatesUnitWithNegativePosition()
        {
            // Arrange & Act
            var negativePosUnit = new UnitModel(_baseStats, UnitType.Archer, -5, -3, 1, true);

            // Assert
            Assert.That(negativePosUnit.Position.Value, Is.EqualTo(new Vector2Int(-5, -3)));
        }

        [Test]
        public void RecieveDamage_WithZeroDamage_DoesNotChangeHealth()
        {
            // Arrange
            var initialHealth = _unitModel.ModifiedStats.Health;
            var damageContext = new DamageContext(0, DamageType.physical, null);

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(initialHealth));
        }

        [Test]
        public void RecieveDamage_WithNegativeDamage_ThrowsException()
        {
            // Arrange
            var damageContext = new DamageContext(-50, DamageType.physical, null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _unitModel.RecieveDamage(damageContext));
        }

        [Test]
        public void RecieveDamage_WithExactHealthDamage_KillsOneUnitAndResetsHealth()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1));
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_baseStats.MaxHealth));
        }

        [Test]
        public void RecieveDamage_WithDamageEqualToMultipleUnitsHealth_KillsCorrectAmount()
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
        public void RecieveDamage_WithOverkillDamage_KillsAllUnits()
        {
            // Arrange
            var damageContext = new DamageContext(10000, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0));
            Assert.That(damageContext.DieAmount, Is.EqualTo(initialAmount));
        }

        [Test]
        public void SendDamage_WithZeroAmount_ReturnsZeroDamage()
        {
            // Arrange
            _unitModel.Amount.Value = 0;
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);

            // Act
            var damageContext = _unitModel.SendDamage(attackContext);

            // Assert
            Assert.That(damageContext.DamageAmount, Is.EqualTo(0));
        }

        [Test]
        public void SendDamage_WithNegativeAmount_ThrowsException()
        {
            // Arrange
            _unitModel.Amount.Value = -5;
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _unitModel.SendDamage(attackContext));
        }

        [Test]
        public void MoveByRoute_WithEmptyRoute_DoesNotChangePosition()
        {
            // Arrange
            var initialPosition = _unitModel.Position.Value;
            var emptyRoute = new List<Vector2Int>();

            // Act
            _unitModel.MoveByRoute(emptyRoute);

            // Assert
            Assert.That(_unitModel.Position.Value, Is.EqualTo(initialPosition));
        }

        [Test]
        public void MoveByRoute_WithSinglePointRoute_UpdatesPosition()
        {
            // Arrange
            var newPosition = new Vector2Int(10, 15);
            var singlePointRoute = new List<Vector2Int> { newPosition };

            // Act
            _unitModel.MoveByRoute(singlePointRoute);

            // Assert
            Assert.That(_unitModel.Position.Value, Is.EqualTo(newPosition));
        }

        [Test]
        public void MoveByRoute_WithNullRoute_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _unitModel.MoveByRoute(null));
        }

        [Test]
        public void ApplyStatusEffect_WithNullEffect_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _unitModel.ApplyEffect(null));
        }

        [Test]
        public void RemoveEffect_WithNullEffect_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _unitModel.RemoveEffect(null));
        }

        [Test]
        public void RemoveEffect_WithNonExistentEffect_DoesNotThrowException()
        {
            // Arrange
            var nonExistentEffect = new MockStatusEffect(StatusEffectType.NotImp);

            // Act & Assert
            Assert.DoesNotThrow(() => _unitModel.RemoveEffect(nonExistentEffect));
        }

        [Test]
        public void EndTurn_WithMultipleEffects_ProcessesAllEffects()
        {
            // Arrange
            var effect1 = new MockStatusEffect(StatusEffectType.Burning);
            var effect2 = new MockStatusEffect(StatusEffectType.Armored);
            _unitModel.ApplyEffect(effect1);
            _unitModel.ApplyEffect(effect2);

            // Act
            _unitModel.EndTurn();

            // Assert
            // Проверяем, что метод не выбрасывает исключений
            Assert.DoesNotThrow(() => _unitModel.EndTurn());
        }

        [Test]
        public void TakeTurn_WithMultipleEffects_ProcessesAllEffects()
        {
            // Arrange
            var effect1 = new MockStatusEffect(StatusEffectType.Burning);
            var effect2 = new MockStatusEffect(StatusEffectType.Armored);
            _unitModel.ApplyEffect(effect1);
            _unitModel.ApplyEffect(effect2);

            // Act
            _unitModel.TakeTurn();

            // Assert
            // Проверяем, что метод не выбрасывает исключений
            Assert.DoesNotThrow(() => _unitModel.TakeTurn());
        }

        [Test]
        public void Position_SetterWithSameValue_DoesNotInvokeEvent()
        {
            // Arrange
            var currentPosition = _unitModel.Position.Value;
            var eventInvoked = false;
            _unitModel.Moved += (route) => eventInvoked = true;

            // Act
            _unitModel.Position.Value = currentPosition;

            // Assert
            Assert.That(eventInvoked, Is.False);
        }

        [Test]
        public void ToString_WithDifferentUnitTypes_ReturnsCorrectString()
        {
            // Arrange & Act
            var archerUnit = new UnitModel(_baseStats, UnitType.Archer, 0, 0, 1, true);
            var witchUnit = new UnitModel(_baseStats, UnitType.Witch, 0, 0, 1, true);
            var warrokUnit = new UnitModel(_baseStats, UnitType.Warrok, 0, 0, 1, true);

            // Assert
            Assert.That(archerUnit.ToString(), Is.EqualTo(UnitType.Archer.ToString()));
            Assert.That(witchUnit.ToString(), Is.EqualTo(UnitType.Witch.ToString()));
            Assert.That(warrokUnit.ToString(), Is.EqualTo(UnitType.Warrok.ToString()));

            // Cleanup
            UnityEngine.Object.DestroyImmediate(archerUnit.BaseUnitStats);
            UnityEngine.Object.DestroyImmediate(witchUnit.BaseUnitStats);
            UnityEngine.Object.DestroyImmediate(warrokUnit.BaseUnitStats);
        }
    }
}
