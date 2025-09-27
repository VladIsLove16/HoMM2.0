using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniRx;

namespace Tests.EditMode.UI
{
    [TestFixture]
    public class UnitViewUITests_EditMode
    {
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
        private UnitStats _baseStats;
        private MaterialProvider _materialProvider;

        [SetUp]
        public void SetUp()
        {
            // Создаем базовые характеристики для тестов
            _baseStats = ScriptableObject.CreateInstance<UnitStats>();
            _baseStats.Health = 100;
            _baseStats.MaxHealth = 100;
            _baseStats.Damage = 25;
            _baseStats.InvulnerableEffects = new List<StatusEffectType>();

            // Создаем модель юнита
            _unitModel = new UnitModel(_baseStats, UnitType.Archer, 5, 3, 10, true);

            // Создаем MaterialProvider
            _materialProvider = new MaterialProvider();

            // Создаем ViewModel
            _unitViewModel = new UnitViewModel(_unitModel);
        }

        [TearDown]
        public void TearDown()
        {
            if (_unitModel?.BaseUnitStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_unitModel.BaseUnitStats);
            }
        }

        [Test]
        public void UpdateHealth_WithFullHealth_CalculatesCorrectRatio()
        {
            // Arrange
            var health = _unitViewModel.Model.ModifiedStats.Health;
            var maxHealth = _unitViewModel.Model.ModifiedStats.MaxHealth;

            // Act
            float ratio = maxHealth > 0 ? (float)health / maxHealth : 0f;

            // Assert
            Assert.That(health, Is.EqualTo(100));
            Assert.That(maxHealth, Is.EqualTo(100));
            Assert.That(ratio, Is.EqualTo(1f));
        }

        [Test]
        public void UpdateHealth_WithPartialDamage_CalculatesCorrectRatio()
        {
            // Arrange
            var damageContext = new DamageContext(25, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            // Act
            var health = _unitViewModel.Model.ModifiedStats.Health;
            var maxHealth = _unitViewModel.Model.ModifiedStats.MaxHealth;
            float ratio = (float)health / maxHealth;

            // Assert
            Assert.That(health, Is.EqualTo(75));
            Assert.That(ratio, Is.EqualTo(0.75f));
        }

        [Test]
        public void UpdateHealth_WithZeroMaxHealth_HandlesDivisionByZero()
        {
            // Arrange
            _unitViewModel.Model.ModifiedStats.MaxHealth = 0;

            // Act
            var health = _unitViewModel.Model.ModifiedStats.Health;
            var maxHealth = _unitViewModel.Model.ModifiedStats.MaxHealth;
            float ratio = maxHealth > 0 ? (float)health / maxHealth : 0f;

            // Assert
            Assert.That(health, Is.EqualTo(0f));
            Assert.That(maxHealth, Is.EqualTo(0f));
            Assert.That(ratio, Is.EqualTo(0f));
        }

        [Test]
        public void UpdateAmount_WithValidAmount_ReturnsCorrectValue()
        {
            // Arrange
            var amount = _unitViewModel.Model.Amount.Value;

            // Act & Assert
            Assert.That(amount, Is.EqualTo(10));
        }

        [Test]
        public void UpdateAmount_AfterUnitDeath_ReturnsCorrectValue()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null);

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0));
        }

        [Test]
        public void OnHealthChanged_WhenModelHealthChanges_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnHealthChanged.Subscribe( _ => eventReceived = true);

            // Act
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True);
        }

        [Test]
        public void OnDeath_WhenModelDies_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnDeath.Subscribe(_ => eventReceived = true);

            // Act
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True);
        }

        [Test]
        public void MultipleHealthChanges_CalculateCorrectly()
        {
            // Arrange
            var damage1 = new DamageContext(20, DamageType.physical, null);
            var damage2 = new DamageContext(30, DamageType.physical, null);
            var damage3 = new DamageContext(25, DamageType.physical, null);

            // Act
            _unitModel.RecieveDamage(damage1);
            _unitModel.RecieveDamage(damage2);
            _unitModel.RecieveDamage(damage3);

            // Assert
            var health = _unitViewModel.Model.ModifiedStats.Health;
            Assert.That(health, Is.EqualTo(25)); // 100 - 20 - 30 - 25 = 25
        }

        [Test]
        public void HealthRatio_WithVariousValues_CalculatesCorrectly()
        {
            // Arrange
            var testCases = new[]
            {
                new { Current = 100, Max = 100, Expected = 1.0f },
                new { Current = 75, Max = 100, Expected = 0.75f },
                new { Current = 50, Max = 100, Expected = 0.5f },
                new { Current = 25, Max = 100, Expected = 0.25f },
                new { Current = 0, Max = 100, Expected = 0.0f }
            };

            // Act & Assert
            foreach (var testCase in testCases)
            {
                float ratio = testCase.Max > 0 ? (float)testCase.Current / testCase.Max : 0f;
                Assert.That(ratio, Is.EqualTo(testCase.Expected), 
                    $"Failed for Current={testCase.Current}, Max={testCase.Max}");
            }
        }
    }
}
