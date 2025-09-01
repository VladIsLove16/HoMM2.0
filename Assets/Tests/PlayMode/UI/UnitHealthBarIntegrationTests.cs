using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using UniRx;

namespace Tests.PlayMode.UI
{
    public class UnitHealthBarIntegrationTests
    {
        private GameObject _gameObject;
        private UnitViewUI _unitViewUI;
        private UnitHealthBar _healthBar;
        private TextMeshProUGUI _amountText;
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
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

            // Создаем модель юнита
            _unitModel = new UnitModel(_baseStats, UnitType.Archer, 5, 3, 10, true);

            // Создаем MaterialProvider
            _materialProvider = new MaterialProvider();

            // Создаем ViewModel
            _unitViewModel = new UnitViewModel(_unitModel, _materialProvider);

            // Создаем GameObject с компонентами
            _gameObject = new GameObject("TestUnitViewUI");
            
            // Добавляем Canvas для UI элементов
            var canvas = _gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _gameObject.AddComponent<CanvasScaler>();
            _gameObject.AddComponent<GraphicRaycaster>();

            // Создаем UnitHealthBar
            var healthBarGameObject = new GameObject("HealthBar");
            healthBarGameObject.transform.SetParent(_gameObject.transform);
            _healthBar = healthBarGameObject.AddComponent<UnitHealthBar>();

            // Создаем Text для количества юнитов
            var amountTextGameObject = new GameObject("AmountText");
            amountTextGameObject.transform.SetParent(_gameObject.transform);
            _amountText = amountTextGameObject.AddComponent<TextMeshProUGUI>();

            // Добавляем UnitViewUI
            _unitViewUI = _gameObject.AddComponent<UnitViewUI>();

            // Устанавливаем ссылки через reflection для тестов
            var healthBarField = typeof(UnitViewUI).GetField("healthBar", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthBarField?.SetValue(_unitViewUI, _healthBar);

            var amountTextField = typeof(UnitViewUI).GetField("amountText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            amountTextField?.SetValue(_unitViewUI, _amountText);

            yield return null; // Ждем один кадр для инициализации
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_unitModel?.BaseUnitStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_unitModel.BaseUnitStats);
            }

            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_Initialization_WorksCorrectly()
        {
            // Act
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f); // Ждем обработки событий

            // Assert
            Assert.That(_amountText.text, Is.EqualTo("10"));
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(100));
            Assert.That(_unitModel.ModifiedStats.MaxHealth, Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_HealthChange_UpdatesCorrectly()
        {
            // Arrange
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Наносим урон
            var damageContext = new DamageContext(30, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f); // Ждем обработки событий

            // Assert
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(70));
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(10)); // Количество не изменилось
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_UnitDeath_UpdatesAmountCorrectly()
        {
            // Arrange
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Наносим смертельный урон
            var damageContext = new DamageContext(150, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f); // Ждем обработки событий

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(9)); // Один юнит умер
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(50)); // Осталось 50 здоровья
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_MultipleDamage_HandlesCorrectly()
        {
            // Arrange
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Наносим несколько ударов
            var damage1 = new DamageContext(25, DamageType.physical, null);
            var damage2 = new DamageContext(35, DamageType.physical, null);
            var damage3 = new DamageContext(20, DamageType.physical, null);

            _unitModel.RecieveDamage(damage1);
            yield return new WaitForSeconds(0.05f);

            _unitModel.RecieveDamage(damage2);
            yield return new WaitForSeconds(0.05f);

            _unitModel.RecieveDamage(damage3);
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(20)); // 100 - 25 - 35 - 20 = 20
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(10)); // Количество не изменилось
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_CompleteUnitDeath_HandlesCorrectly()
        {
            // Arrange
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Убиваем всех юнитов
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f); // Ждем обработки событий

            // Assert
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0));
            Assert.That(_amountText.color, Is.EqualTo(Color.black)); // Цвет должен стать черным
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_HealthRatio_CalculatesCorrectly()
        {
            // Arrange
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Наносим урон в 50 единиц
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert
            var health = _unitModel.ModifiedStats.Health;
            var maxHealth = _unitModel.ModifiedStats.MaxHealth;
            float ratio = (float)health / maxHealth;

            Assert.That(health, Is.EqualTo(50));
            Assert.That(ratio, Is.EqualTo(0.5f));
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_EdgeCase_ZeroMaxHealth()
        {
            // Arrange
            _baseStats.MaxHealth = 0; // Устанавливаем максимальное здоровье в 0
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Проверяем обработку деления на ноль
            var health = _unitViewModel.Model.ModifiedStats.Health;
            var maxHealth = _unitViewModel.Model.ModifiedStats.MaxHealth;
            float ratio = maxHealth > 0 ? (float)health / maxHealth : 0f;

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.That(ratio, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_EventSubscription_WorksCorrectly()
        {
            // Arrange
            var healthChangedReceived = false;
            var deathReceived = false;

            _unitViewModel.OnHealthChanged.Subscribe(_ => healthChangedReceived = true);
            _unitViewModel.OnDeath.Subscribe(_ => deathReceived = true);

            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Наносим урон
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.That(healthChangedReceived, Is.True);
            Assert.That(deathReceived, Is.False); // Юнит не умер
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_DeathEvent_TriggersCorrectly()
        {
            // Arrange
            var deathReceived = false;
            _unitViewModel.OnDeath.Subscribe(_ => deathReceived = true);

            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act - Убиваем всех юнитов
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.That(deathReceived, Is.True);
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_Dispose_WorksCorrectly()
        {
            // Arrange
            _unitViewUI.Init(_unitViewModel);

            yield return new WaitForSeconds(0.1f);

            // Act
            _unitViewUI.Dispose();

            yield return new WaitForSeconds(0.1f);

            // Assert - Проверяем, что Dispose не вызывает исключений
            Assert.DoesNotThrow(() => _unitViewUI.Dispose());
        }
    }
}
