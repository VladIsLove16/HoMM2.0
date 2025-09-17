using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace Tests.PlayMode
{
    public class UI_HealthRatio_Update_Tests : ZenjectIntegrationTestFixture
    {
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
        private UnitViewUI _unitViewUI;
        private UnitHealthBar _healthBar;
        private TextMeshProUGUI _amountText;
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

            // Создаем Canvas для UI тестов
            var canvasGameObject = new GameObject("TestCanvas");
            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();

            // Создаем UnitHealthBar
            var healthBarGameObject = new GameObject("HealthBar");
            healthBarGameObject.transform.SetParent(canvasGameObject.transform);
            _healthBar = healthBarGameObject.AddComponent<UnitHealthBar>();
            
            var healthImage = healthBarGameObject.AddComponent<Image>();
            healthImage.type = Image.Type.Filled;
            healthImage.fillMethod = Image.FillMethod.Horizontal;
            
            var healthText = healthBarGameObject.AddComponent<TextMeshProUGUI>();
            
            // Устанавливаем ссылки через reflection
            var healthTextField = typeof(UnitHealthBar).GetField("HealthText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthTextField?.SetValue(_healthBar, healthText);

            var healthImageField = typeof(UnitHealthBar).GetField("HealthImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthImageField?.SetValue(_healthBar, healthImage);

            // Создаем UnitViewUI
            var unitViewUIGameObject = new GameObject("UnitViewUI");
            unitViewUIGameObject.transform.SetParent(canvasGameObject.transform);
            _unitViewUI = unitViewUIGameObject.AddComponent<UnitViewUI>();
            
            var amountTextGameObject = new GameObject("AmountText");
            amountTextGameObject.transform.SetParent(unitViewUIGameObject.transform);
            _amountText = amountTextGameObject.AddComponent<TextMeshProUGUI>();
            
            // Устанавливаем ссылки через reflection
            var healthBarField = typeof(UnitViewUI).GetField("healthBar", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthBarField?.SetValue(_unitViewUI, _healthBar);

            var amountTextField = typeof(UnitViewUI).GetField("amountText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            amountTextField?.SetValue(_unitViewUI, _amountText);

            // Инициализируем компоненты
            _healthBar.Init();
            _unitViewUI.Init(_unitViewModel);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_unitModel?.BaseUnitStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_unitModel.BaseUnitStats);
            }
            
            if (_healthBar != null)
            {
                UnityEngine.Object.DestroyImmediate(_healthBar.gameObject);
            }
            
            if (_unitViewUI != null)
            {
                UnityEngine.Object.DestroyImmediate(_unitViewUI.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HealthBar_InitialState_ShowsFullHealth()
        {
            // Arrange & Act - Инициализация уже произошла в SetUp

            // Assert - Полоска здоровья должна показывать полное здоровье
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(1.0f), 
                "Health bar should show full health initially");
            yield return new WaitForSeconds(0.1f);
        }

        [UnityTest]
        public IEnumerator HealthBar_AfterDamage_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var expectedRatio = 0.5f; // 50/100

            // Act - Наносим урон
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Полоска здоровья должна обновиться
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(expectedRatio), 
                "Health bar should update to show 50% health after damage");
        }

        [UnityTest]
        public IEnumerator HealthBar_AfterUnitDeath_ResetsToFullHealth()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);

            // Act - Убиваем одного юнита
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Полоска здоровья должна сброситься к полному здоровью
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(1.0f), 
                "Health bar should reset to full health after unit death");
        }

        [UnityTest]
        public IEnumerator AmountText_InitialState_ShowsCorrectAmount()
        {
            // Arrange & Act - Инициализация уже произошла в SetUp

            // Assert - Текст количества должен показывать правильное количество
            Assert.That(_amountText.text, Is.EqualTo(_unitModel.Amount.Value.ToString()), 
                "Amount text should show correct initial amount");
            yield return new WaitForSeconds(0.1f);
        }

        [UnityTest]
        public IEnumerator AmountText_AfterUnitDeath_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;
            var expectedAmount = initialAmount - 1;

            // Act - Убиваем одного юнита
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Текст количества должен обновиться
            Assert.That(_amountText.text, Is.EqualTo(expectedAmount.ToString()), 
                "Amount text should update to show decreased amount after unit death");
        }

        [UnityTest]
        public IEnumerator HealthBar_WithMultipleUnitDeaths_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(250, DamageType.physical, null);
            var expectedRatio = 0.5f; // 50/100 после убийства 2 юнитов

            // Act - Убиваем 2 юнита и повреждаем третьего
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Полоска здоровья должна показать правильное соотношение
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(expectedRatio), 
                "Health bar should show 50% health after multiple unit deaths");
        }

        [UnityTest]
        public IEnumerator UI_WithCompleteDestruction_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null);

            // Act - Убиваем всех юнитов
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - UI должен показать полное уничтожение
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(0.0f), 
                "Health bar should show 0% health when all units are dead");
            Assert.That(_amountText.text, Is.EqualTo("0"), 
                "Amount text should show 0 when all units are dead");
        }

        [UnityTest]
        public IEnumerator UI_WithPartialDamage_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(30, DamageType.physical, null);
            var expectedRatio = 0.7f; // 70/100

            // Act - Наносим небольшой урон
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - UI должен обновиться, но количество не изменится
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(expectedRatio), 
                "Health bar should show 70% health after partial damage");
            Assert.That(_amountText.text, Is.EqualTo(_unitModel.Amount.Value.ToString()), 
                "Amount text should remain unchanged for non-lethal damage");
        }

        [UnityTest]
        public IEnumerator UI_WithExactKill_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act - Убиваем точно одного юнита
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - UI должен обновиться корректно
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(1.0f), 
                "Health bar should show full health after exact kill");
            Assert.That(_amountText.text, Is.EqualTo((initialAmount - 1).ToString()), 
                "Amount text should show decreased amount after exact kill");
        }

        [UnityTest]
        public IEnumerator UI_WithMultipleSmallDamages_UpdatesCorrectly()
        {
            // Arrange
            var damage1 = new DamageContext(30, DamageType.physical, null);
            var damage2 = new DamageContext(40, DamageType.physical, null);
            var damage3 = new DamageContext(50, DamageType.physical, null);

            // Act - Наносим несколько небольших уронов
            _unitModel.RecieveDamage(damage1); // 100 - 30 = 70
            yield return new WaitForSeconds(0.1f);
            
            _unitModel.RecieveDamage(damage2); // 70 - 40 = 30
            yield return new WaitForSeconds(0.1f);
            
            _unitModel.RecieveDamage(damage3); // 30 - 50 = -20, убиваем юнита
            yield return new WaitForSeconds(0.1f);

            // Assert - UI должен обновиться после каждого урона
            var healthImage = _healthBar.GetComponent<Image>();
            Assert.That(healthImage.fillAmount, Is.EqualTo(0.8f), 
                "Health bar should show 80% health after accumulated damage");
        }
    }
}

