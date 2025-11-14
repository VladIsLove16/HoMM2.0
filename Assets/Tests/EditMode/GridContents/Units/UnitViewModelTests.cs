using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniRx;
using Zenject;
using Tests.Common;

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
            _unitViewModel = new UnitViewModel(_unitModel, _worldToCellProvider);
            
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
        }

        [Test]
        public void Constructor_WithBlueTeamUnit_InitializesCorrectly()
        {
            // Arrange
            var blueTeamStats = ScriptableObject.CreateInstance<UnitStats>();
            blueTeamStats.InvulnerableEffects = new List<StatusEffectType>();
            var blueTeamModel = new UnitModel(blueTeamStats, UnitType.Archer, 0, 0, 1, true);
            var blueTeamViewModel = new UnitViewModel(blueTeamModel, new MockWorldToCellProvider());

            // Assert
            Assert.That(blueTeamViewModel.Model, Is.EqualTo(blueTeamModel));
        }

        [Test]
        public void Constructor_WithRedTeamUnit_InitializesCorrectly()
        {
            // Arrange
            var redTeamStats = ScriptableObject.CreateInstance<UnitStats>();
            redTeamStats.InvulnerableEffects = new List<StatusEffectType>();
            var redTeamModel = new UnitModel(redTeamStats, UnitType.Archer, 0, 0, 1, false);
            var redTeamViewModel = new UnitViewModel(redTeamModel, new MockWorldToCellProvider());

            // Assert
            Assert.That(redTeamViewModel.Model, Is.EqualTo(redTeamModel));
        }

        [Test]
        public void OnAttackedWorld_WhenModelAttacked_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            Vector3 receivedPosition = default;
            _unitViewModel.OnAttackedWorld.Subscribe(pos =>
            {
                eventReceived = true;
                receivedPosition = pos;
            });

            // Act
            var mockTarget = new MockDamagable { Position = new Vector2Int(2, 1) };
            var attackContext = new AttackContext(mockTarget);
            _unitModel.SendDamage(attackContext);

            // Assert
            Assert.That(eventReceived, Is.True);
            Assert.That(receivedPosition, Is.EqualTo(_worldToCellProvider.ToWorld(mockTarget.Position.x, mockTarget.Position.y)));
        }

        [Test]
        public void OnHitWorld_WhenModelHit_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            Vector3 receivedPosition = default;
            _unitViewModel.OnHitWorld.Subscribe(pos =>
            {
                eventReceived = true;
                receivedPosition = pos;
            });

            // Act
            var attacker = new MockDamageSource { Position = new Vector2Int(4, 2) };
            var damageContext = new DamageContext(50, DamageType.physical, attacker);
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True);
            Assert.That(receivedPosition, Is.EqualTo(_worldToCellProvider.ToWorld(attacker.Position.x, attacker.Position.y)));
        }

        [Test]
        public void OnDeath_WhenModelDies_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnDeath.Subscribe(_ => eventReceived = true);

            // Act
            var damageContext = new DamageContext(1000, DamageType.physical, new MockDamageSource());
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
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True);
        }

        [Test]
        public void OnAttackedWorld_WithMultipleSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var event1Received = false;
            var event2Received = false;
            var event3Received = false;

            _unitViewModel.OnAttackedWorld.Subscribe(_ => event1Received = true);
            _unitViewModel.OnAttackedWorld.Subscribe(_ => event2Received = true);
            _unitViewModel.OnAttackedWorld.Subscribe(_ => event3Received = true);

            // Act
            var mockTarget = new MockDamagable { Position = new Vector2Int(3, 0) };
            var attackContext = new AttackContext(mockTarget);
            _unitModel.SendDamage(attackContext);

            // Assert
            Assert.That(event1Received, Is.True);
            Assert.That(event2Received, Is.True);
            Assert.That(event3Received, Is.True);
        }

        [Test]
        public void OnHitWorld_WithMultipleSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var event1Received = false;
            var event2Received = false;

            _unitViewModel.OnHitWorld.Subscribe(_ => event1Received = true);
            _unitViewModel.OnHitWorld.Subscribe(_ => event2Received = true);

            // Act
            var attacker = new MockDamageSource { Position = new Vector2Int(1, 5) };
            var damageContext = new DamageContext(50, DamageType.physical, attacker);
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
            var damageContext = new DamageContext(1000, DamageType.physical, new MockDamageSource());
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
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(event1Received, Is.True);
            Assert.That(event2Received, Is.True);
        }

        [Test]
        public void Events_AreUniRxObservables()
        {
            // Assert
            Assert.That(_unitViewModel.OnAttackedWorld, Is.InstanceOf<IObservable<Vector3>>());
            Assert.That(_unitViewModel.OnHitWorld, Is.InstanceOf<IObservable<Vector3>>());
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
            Assert.That(_unitViewModel.Model.Team.Value, Is.EqualTo(Team.Blue));
        }


        [Test]
        public void OnDeath_WhenModelPartiallyDamaged_DoesNotEmitEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnDeath.Subscribe(_ => eventReceived = true);

            // Act - Урон меньше здоровья, не убивает юнита
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.False, "OnDeath should not be emitted for non-lethal damage");
        }

        [Test]
        public void OnDeath_WhenModelFullyKilled_EmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnDeath.Subscribe(_ => eventReceived = true);

            // Act - Смертельный урон
            var damageContext = new DamageContext(1000, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True, "OnDeath should be emitted for lethal damage");
        }

        [Test]
        public void OnHitWorld_WhenModelDamaged_AlwaysEmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnHitWorld.Subscribe(_ => eventReceived = true);

            // Act - Любой урон
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True, "OnHitWorld should always be emitted when damage is received");
        }

        [Test]
        public void OnHealthChanged_WhenModelDamaged_AlwaysEmitsEvent()
        {
            // Arrange
            var eventReceived = false;
            _unitViewModel.OnHealthChanged.Subscribe(_ => eventReceived = true);

            // Act - Любой урон
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(eventReceived, Is.True, "OnHealthChanged should always be emitted when health changes");
        }

        [Test]
        public void EventSequence_WhenModelDamaged_EmitsInCorrectOrder()
        {
            // Arrange
            var eventSequence = new List<string>();
            
            _unitViewModel.OnHealthChanged.Subscribe(_ => eventSequence.Add("OnHealthChanged"));
            _unitViewModel.OnHitWorld.Subscribe(_ => eventSequence.Add("OnHitWorld"));
            _unitViewModel.OnDeath.Subscribe(_ => eventSequence.Add("OnDeath"));

            // Act - Урон меньше здоровья
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert - События должны вызываться в правильном порядке
            Assert.That(eventSequence.Count, Is.EqualTo(2), "Should emit 2 events for non-lethal damage");
            Assert.That(eventSequence[0], Is.EqualTo("OnHealthChanged"), "OnHealthChanged should be first");
            Assert.That(eventSequence[1], Is.EqualTo("OnHitWorld"), "OnHitWorld should be second");
        }

        [Test]
        public void EventSequence_WhenModelKilled_EmitsInCorrectOrder()
        {
            // Arrange
            var eventSequence = new List<string>();
            
            _unitViewModel.OnHealthChanged.Subscribe(_ => eventSequence.Add("OnHealthChanged"));
            _unitViewModel.OnHitWorld.Subscribe(_ => eventSequence.Add("OnHitWorld"));
            _unitViewModel.OnDeath.Subscribe(_ => eventSequence.Add("OnDeath"));

            // Act - Смертельный урон
            var damageContext = new DamageContext(1000, DamageType.physical, new MockDamageSource());
            _unitModel.RecieveDamage(damageContext);

            // Assert - События должны вызываться в правильном порядке
            Assert.That(eventSequence.Count, Is.EqualTo(3), "Should emit 3 events for lethal damage");
            Assert.That(eventSequence[0], Is.EqualTo("OnHealthChanged"), "OnHealthChanged should be first");
            Assert.That(eventSequence[1], Is.EqualTo("OnHitWorld"), "OnHitWorld should be second");
            Assert.That(eventSequence[2], Is.EqualTo("OnDeath"), "OnDeath should be last");
        }

        [Test]
        public void HealthRatio_WhenModelDamaged_CalculatesCorrectly()
        {
            // Arrange
            var initialHealth = _unitModel.ModifiedStats.Health;
            var maxHealth = _unitModel.ModifiedStats.MaxHealth;
            var expectedRatio = (float)initialHealth / maxHealth;

            // Act
            var actualRatio = _unitViewModel.HealthRatio;

            // Assert
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be calculated correctly");
            Assert.That(actualRatio, Is.EqualTo(1.0f), 
                "Initial health ratio should be 1.0 (100%)");
        }

        [Test]
        public void HealthRatio_AfterPartialDamage_CalculatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
            var initialHealth = _unitModel.ModifiedStats.Health;

            // Act
            _unitModel.RecieveDamage(damageContext);
            var actualRatio = _unitViewModel.HealthRatio;
            var expectedRatio = (float)_unitModel.ModifiedStats.Health / _unitModel.ModifiedStats.MaxHealth;

            // Assert
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be calculated correctly after damage");
            Assert.That(actualRatio, Is.EqualTo(0.5f), 
                "Health ratio should be 0.5 (50%) after 50 damage to 100 health");
        }

        [Test]
        public void HealthRatio_AfterUnitDeath_CalculatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, new MockDamageSource());
            var initialAmount = _unitModel.Amount.Value;

            // Act
            _unitModel.RecieveDamage(damageContext);
            var actualRatio = _unitViewModel.HealthRatio;
            var expectedRatio = (float)_unitModel.ModifiedStats.Health / _unitModel.ModifiedStats.MaxHealth;

            // Assert
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be calculated correctly after unit death");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_unitModel.BaseUnitStats.MaxHealth), 
                "Remaining unit should have full health");
        }

        [Test]
        public void HealthRatio_WithMultipleUnitDeaths_CalculatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(150, DamageType.physical, new MockDamageSource());
            var initialAmount = _unitModel.Amount.Value;

            // Act - Убиваем одного юнита и повреждаем второго
            _unitModel.RecieveDamage(damageContext);
            var actualRatio = _unitViewModel.HealthRatio;
            var expectedRatio = (float)_unitModel.ModifiedStats.Health / _unitModel.ModifiedStats.MaxHealth;

            // Assert
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be calculated correctly after multiple unit deaths");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(50), 
                "Remaining unit should have 50 health (100 - 50)");
        }
        [Test]
        public void UnitAmount_WhenModelDamaged_UpdatesCorrectly()
        {
            // Arrange
            var initialAmount = _unitModel.Amount.Value;
            var damageContext = new DamageContext(100, DamageType.physical, new MockDamageSource());

            // Act
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1 after lethal damage");
        }

        [Test]
        public void UnitAmount_WithMultipleDeaths_UpdatesCorrectly()
        {
            // Arrange
            var initialAmount = _unitModel.Amount.Value;
            var damageContext = new DamageContext(250, DamageType.physical, new MockDamageSource());

            // Act - Убиваем 2 юнита (100 + 100 = 200) и повреждаем третьего на 50
            _unitModel.RecieveDamage(damageContext);

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 2), 
                "Unit amount should decrease by 2 after killing 2 units");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(50), 
                "Remaining unit should have 50 health (100 - 50)");
        }

        [Test]
        public void HealthRatio_WithStatusEffects_CalculatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());

            // Act
            _unitModel.RecieveDamage(damageContext);
            var actualRatio = _unitViewModel.HealthRatio;
            var expectedRatio = (float)_unitModel.ModifiedStats.Health / _unitModel.ModifiedStats.MaxHealth;

            // Assert
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be calculated correctly with status effects");
            Assert.That(actualRatio, Is.EqualTo(0.5f), 
                "Health ratio should be 0.5 (50%) after 50 damage");
        }

        //[Test]
        //public void HealthRatio_WithHealing_CalculatesCorrectly()
        //{
        //    // Arrange
        //    var damageContext = new DamageContext(50, DamageType.physical, new MockDamageSource());
        //    _unitModel.RecieveDamage(damageContext);
        //    var ratioAfterDamage = _unitViewModel.HealthRatio;

        //    // Act - Восстанавливаем здоровье (это происходит автоматически при смерти юнита в стэке)
        //    var ratioAfterHealing = _unitViewModel.HealthRatio;

        //    // Assert
        //    Assert.That(ratioAfterDamage, Is.EqualTo(0.5f), 
        //        "Health ratio should be 0.5 after damage");
        //    Assert.That(ratioAfterHealing, Is.EqualTo(1.0f), 
        //        "Health ratio should be 1.0 after healing (when next unit in stack gets full health)");
        //}

        // Mock классы для тестирования
        private class MockDamagable : IDamagable
        {
            public Team Team => Team.Blue;
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;

            public void RecieveDamage(DamageContext ctx) { }
            public void SimulateRecieveDamage(DamageContext ctx) { }
        }

        private class MockDamageSource : IDamageSource, IGridContent
        {
            public Vector2Int Position { get; set; } = Vector2Int.zero;
            public Team Team => Team.Red;
            public GridContentType GridContentType => GridContentType.unit;

            public DamageContext SendDamage(AttackContext ctx) => throw new NotImplementedException();
            public DamageContext SimulateSendDamage(AttackContext ctx) => throw new NotImplementedException();
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

            public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
            {
                var key = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
                var value = new Vector2Int(Mathf.RoundToInt(position.x-1), Mathf.RoundToInt(position.z));
                coords = new(key, value);
                return true;
            }
        }
    }
}



