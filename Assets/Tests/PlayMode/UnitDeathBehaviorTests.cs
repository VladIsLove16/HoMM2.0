using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

namespace Tests.PlayMode
{
    public class UnitDeathBehaviorTests : ZenjectIntegrationTestFixture
    {
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
        private UnitView3D _unitView3D;
        private UnitStats _baseStats;
        private MaterialProvider _materialProvider;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Создаем базовые характеристики для тестов
            _baseStats = ScriptableObject.CreateInstance<UnitStats>();
            _baseStats.Health = 100;
            _baseStats.MaxHealth = 100;
            _baseStats.Damage = 25;
            _baseStats.InvulnerableEffects = new List<StatusEffectType>();

            // Создаем модель юнита с несколькими юнитами в стэке
            _unitModel = new UnitModel(_baseStats, UnitType.Archer, 5, 3, 3, true); // 3 юнита в стэке

            // Создаем MaterialProvider
            _materialProvider = new MaterialProvider();

            // Создаем ViewModel
            _unitViewModel = new UnitViewModel(_unitModel, _materialProvider);

            // Создаем GameObject с UnitView3D
            var gameObject = new GameObject("TestUnit");
            _unitView3D = gameObject.AddComponent<UnitView3D>();
            
            // Добавляем Animator для тестов
            var animator = gameObject.AddComponent<Animator>();
            
            // Инициализируем UnitView3D
            _unitView3D.Init(_unitViewModel);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_unitModel?.BaseUnitStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_unitModel.BaseUnitStats);
            }
            
            if (_unitView3D != null)
            {
                UnityEngine.Object.DestroyImmediate(_unitView3D.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitView3D_NonLethalDamage_KeepsGameObjectActive()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null); // Урон меньше здоровья
            var initialAmount = _unitModel.Amount.Value;

            // Act - Атакуем юнита, но не убиваем его полностью
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(2f); // Ждем завершения анимации

            // Assert - GameObject должен остаться активным
            Assert.That(_unitView3D.gameObject.activeSelf, Is.True, 
                "GameObject should remain active after non-lethal damage");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount), 
                "Unit amount should remain the same");
        }

        [UnityTest]
        public IEnumerator UnitView3D_PartialStackDeath_KeepsGameObjectActive()
        {
            // Arrange
            var damageContext = new DamageContext(150, DamageType.physical, null); // Урон больше здоровья одного юнита
            var initialAmount = _unitModel.Amount.Value;

            // Act - Атакуем юнита, убиваем одного, но не весь стэк
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(2f);

            // Assert - GameObject должен остаться активным, количество уменьшилось
            Assert.That(_unitView3D.gameObject.activeSelf, Is.True, 
                "GameObject should remain active after partial stack death");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_unitModel.BaseUnitStats.MaxHealth), 
                "Remaining unit should have full health");
        }

        [UnityTest]
        public IEnumerator UnitView3D_FullDeath_DeactivatesGameObject()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null); // Смертельный урон

            // Act - Убиваем юнита полностью
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(2f); // Ждем завершения анимации смерти

            // Assert - GameObject должен быть деактивирован
            Assert.That(_unitView3D.gameObject.activeSelf, Is.False, 
                "GameObject should be deactivated after unit death");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0), 
                "Unit amount should be 0");
        }

        [UnityTest]
        public IEnumerator UnitView3D_DeathAnimation_PlaysBeforeDeactivation()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null); // Смертельный урон
            var animator = _unitView3D.GetComponent<Animator>();

            // Act - Убиваем юнита полностью
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.5f); // Ждем начала анимации смерти

            // Assert - GameObject еще активен, анимация смерти играется
            Assert.That(_unitView3D.gameObject.activeSelf, Is.True, 
                "GameObject should still be active during death animation");

            yield return new WaitForSeconds(2f); // Ждем завершения анимации

            // Assert - Теперь GameObject деактивирован
            Assert.That(_unitView3D.gameObject.activeSelf, Is.False, 
                "GameObject should be deactivated after death animation");
        }

        [UnityTest]
        public IEnumerator UnitView3D_EventHandling_WorksCorrectly()
        {
            // Arrange
            var onDeathInvoked = false;
            var onHitInvoked = false;

            _unitViewModel.OnDeath.Subscribe(_ => onDeathInvoked = true);
            _unitViewModel.OnHit.Subscribe(_ => onHitInvoked = true);

            // Act - Атакуем юнита, но не убиваем
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - OnHit должен вызваться, OnDeath - нет
            Assert.That(onHitInvoked, Is.True, "OnHit should be invoked");
            Assert.That(onDeathInvoked, Is.False, "OnDeath should not be invoked for non-lethal damage");

            // Act - Теперь убиваем юнита
            var lethalDamageContext = new DamageContext(1000, DamageType.physical, null);
            _unitModel.RecieveDamage(lethalDamageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Теперь OnDeath должен вызваться
            Assert.That(onDeathInvoked, Is.True, "OnDeath should be invoked for lethal damage");
        }

        [UnityTest]
        public IEnumerator UnitView3D_MultipleAttacks_HandlesCorrectly()
        {
            // Arrange
            var attackCount = 0;
            var deathCount = 0;
            
            _unitViewModel.OnHit.Subscribe(_ => attackCount++);
            _unitViewModel.OnDeath.Subscribe(_ => deathCount++);

            // Act - Несколько атак, но не убиваем
            for (int i = 0; i < 3; i++)
            {
                var damageContext = new DamageContext(30, DamageType.physical, null);
                _unitModel.RecieveDamage(damageContext);
                yield return new WaitForSeconds(0.1f);
            }

            // Assert - GameObject должен остаться активным
            Assert.That(_unitView3D.gameObject.activeSelf, Is.True, 
                "GameObject should remain active after multiple non-lethal attacks");
            Assert.That(attackCount, Is.EqualTo(3), "Should receive 3 hit events");
            Assert.That(deathCount, Is.EqualTo(0), "Should not receive death events");

            // Act - Финальная смертельная атака
            var lethalDamageContext = new DamageContext(1000, DamageType.physical, null);
            _unitModel.RecieveDamage(lethalDamageContext);

            yield return new WaitForSeconds(2f);

            // Assert - Теперь GameObject деактивирован
            Assert.That(_unitView3D.gameObject.activeSelf, Is.False, 
                "GameObject should be deactivated after lethal attack");
            Assert.That(deathCount, Is.EqualTo(1), "Should receive 1 death event");
        }
    }
}
