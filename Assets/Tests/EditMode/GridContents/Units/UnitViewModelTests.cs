using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniRx;
using Zenject;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitViewModelTests
    {
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
        private MockWorldToCellProvider _worldToCellProvider;
        private MaterialProvider _materialProvider = new();
        [SetUp]
        public void SetUp()
        {
            // Создаем базовые характеристики для тестов
            var baseStats = ScriptableObject.CreateInstance<UnitStats>();
            baseStats.Health = 100;
            baseStats.MaxHealth = 100;
            baseStats.Damage = 25;
            baseStats.InvulnerableEffects = new List<StatusEffectType>();

            // Создаем модель юнита
            _unitModel = new UnitModel(baseStats, UnitType.Archer, 5, 3, 10, true);

            // Создаем мок провайдера
            _worldToCellProvider = new MockWorldToCellProvider();

            // Создаем ViewModel
            _unitViewModel = new UnitViewModel(_unitModel, _materialProvider);
            
            // Инжектируем зависимости через reflection (для тестов)
            var field = typeof(UnitViewModel).GetField("_worldToCellProvider", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_unitViewModel, _worldToCellProvider);
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
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Assert
            Assert.That(_unitViewModel.Model, Is.EqualTo(_unitModel));
            Assert.That(_unitViewModel.TeamMaterial, Is.Not.Null);
            Assert.That(_unitViewModel.HoveredTeamMaterial, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithBlueTeamUnit_SetsBlueTeamMaterials()
        {
            // Arrange
            var blueTeamStats = ScriptableObject.CreateInstance<UnitStats>();
            blueTeamStats.InvulnerableEffects = new List<StatusEffectType>();
            var blueTeamModel = new UnitModel(blueTeamStats, UnitType.Archer, 0, 0, 1, true);
            var blueTeamViewModel = new UnitViewModel(blueTeamModel, _materialProvider);

            // Assert
            Assert.That(blueTeamViewModel.TeamMaterial, Is.EqualTo(_materialProvider.GetBlueTeamMaterial()));
            Assert.That(blueTeamViewModel.HoveredTeamMaterial, Is.EqualTo(_materialProvider.GetHoveredBlueTeamMaterial()));
        }

        [Test]
        public void Constructor_WithRedTeamUnit_SetsRedTeamMaterials()
        {
            // Arrange
            var redTeamStats = ScriptableObject.CreateInstance<UnitStats>();
            redTeamStats.InvulnerableEffects = new List<StatusEffectType>();
            var redTeamModel = new UnitModel(redTeamStats, UnitType.Archer, 0, 0, 1, false);
            var redTeamViewModel = new UnitViewModel(redTeamModel, _materialProvider);

            // Assert
            Assert.That(redTeamViewModel.TeamMaterial, Is.EqualTo(_materialProvider.GetHoveredRedTeamMaterial()));
            Assert.That(redTeamViewModel.HoveredTeamMaterial, Is.EqualTo(_materialProvider.GetHoveredRedTeamMaterial()));
        }

        [Test]
        public void OnAttacked_WhenModelAttacked_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnAttacked.Subscribe(_ => eventReceived = true);

            // Act
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);
            _unitModel.SendDamage(attackContext);

            // Assert
            Assert.That(eventReceived, Is.True);
        }

        [Test]
        public void OnHit_WhenModelHit_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnHit.Subscribe(_ => eventReceived = true);

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
        public void OnTurnStarted_WhenModelTurnStarts_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnTurnStarted.Subscribe(_ => eventReceived = true);

            // Act
            _unitModel.TakeTurn();

            // Assert
            Assert.That(eventReceived, Is.True);
        }

        [Test]
        public void OnHealthChanged_WhenModelHealthChanges_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnHealthChanged.Subscribe(_ => eventReceived = true);

            // Act
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True);
        }

        [Test]
        public void OnAttacked_WithMultipleSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var event1Received = false;
            var event2Received = false;
            var event3Received = false;

            _unitViewModel.OnAttacked.Subscribe(_ => event1Received = true);
            _unitViewModel.OnAttacked.Subscribe(_ => event2Received = true);
            _unitViewModel.OnAttacked.Subscribe(_ => event3Received = true);

            // Act
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);
            _unitModel.SendDamage(attackContext);

            // Assert
            Assert.That(event1Received, Is.True);
            Assert.That(event2Received, Is.True);
            Assert.That(event3Received, Is.True);
        }

        [Test]
        public void OnHit_WithMultipleSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var event1Received = false;
            var event2Received = false;

            _unitViewModel.OnHit.Subscribe(_ => event1Received = true);
            _unitViewModel.OnHit.Subscribe(_ => event2Received = true);

            // Act
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(event1Received, Is.True);
            Assert.That(event2Received, Is.True);
        }

        [Test]
        public void OnDeath_WithMultipleSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var event1Received = false;
            var event2Received = false;

            _unitViewModel.OnDeath.Subscribe(_ => event1Received = true);
            _unitViewModel.OnDeath.Subscribe(_ => event2Received = true);

            // Act
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(event1Received, Is.True);
            Assert.That(event2Received, Is.True);
        }

        [Test]
        public void OnTurnStarted_WithMultipleSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var event1Received = false;
            var event2Received = false;

            _unitViewModel.OnTurnStarted.Subscribe(_ => event1Received = true);
            _unitViewModel.OnTurnStarted.Subscribe(_ => event2Received = true);

            // Act
            _unitModel.TakeTurn();

            // Assert
            Assert.That(event1Received, Is.True);
            Assert.That(event2Received, Is.True);
        }

        [Test]
        public void OnHealthChanged_WithMultipleSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var event1Received = false;
            var event2Received = false;

            _unitViewModel.OnHealthChanged.Subscribe(_ => event1Received = true);
            _unitViewModel.OnHealthChanged.Subscribe(_ => event2Received = true);

            // Act
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(event1Received, Is.True);
            Assert.That(event2Received, Is.True);
        }

        [Test]
        public void Events_AreUniRxObservables()
        {
            // Assert
            Assert.That(_unitViewModel.OnAttacked, Is.InstanceOf<IObservable<Unit>>());
            Assert.That(_unitViewModel.OnHit, Is.InstanceOf<IObservable<Unit>>());
            Assert.That(_unitViewModel.OnDeath, Is.InstanceOf<IObservable<Unit>>());
            Assert.That(_unitViewModel.OnTurnStarted, Is.InstanceOf<IObservable<Unit>>());
            Assert.That(_unitViewModel.OnHealthChanged, Is.InstanceOf<IObservable<Unit>>());
        }

        [Test]
        public void Model_IsAccessibleAndCorrect()
        {
            // Assert
            Assert.That(_unitViewModel.Model, Is.Not.Null);
            Assert.That(_unitViewModel.Model, Is.EqualTo(_unitModel));
            Assert.That(_unitViewModel.Model.UnitType.Value, Is.EqualTo(UnitType.Archer));
            Assert.That(_unitViewModel.Model.IsBlueTeam.Value, Is.True);
        }

        [Test]
        public void Materials_AreNotNull()
        {
            // Assert
            Assert.That(_unitViewModel.TeamMaterial, Is.Not.Null);
            Assert.That(_unitViewModel.HoveredTeamMaterial, Is.Not.Null);
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

        private class MockUnitDefinitionSO : UnitDefinitionSO
        {
            public Material BlueTeamMaterial => CreateMockMaterial("BlueTeam");
            public Material HoveredBlueTeamMaterial => CreateMockMaterial("HoveredBlueTeam");
            public Material RedTeamMaterial => CreateMockMaterial("RedTeam");
            public Material HoveredRedTeamMaterial => CreateMockMaterial("HoveredRedTeam");

            private Material CreateMockMaterial(string name)
            {
                var material = new Material(Shader.Find("Standard"));
                material.name = name;
                return material;
            }
        }

        private class MockWorldToCellProvider : IWorldToCellProvider
        {
            public Vector3 ToWorld(int x, int y)
            {
                return new Vector3(x, 0, y);
            }

            public bool ToGrid(Vector3 position, out Vector2Int coords)
            {
                coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
                return true;
            }
        }
    }
}
