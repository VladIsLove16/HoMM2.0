using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace Tests.PlayMode.UI
{
    [TestFixture]
    public class UnitViewUI_HealthAndAmount_Tests
    {
        private UnitStats _stats;
        private UnitModel _model;
        private UnitViewModel _vm;
        private GameObject _canvasGO;
        private UnitViewUI _ui;
        private UnitHealthBar _healthBar;
        private TextMeshProUGUI _amountText;
        private TextMeshProUGUI _healthAmountText;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Базовые статы и модель (стэк из 3 юнитов)
            _stats = ScriptableObject.CreateInstance<UnitStats>();
            _stats.Health = 100;
            _stats.MaxHealth = 100;
            _stats.Damage = 25;
            _stats.InvulnerableEffects = new List<StatusEffectType>();
            _model = new UnitModel(_stats, UnitType.Archer, 0, 0, 3, true);
            _vm = new UnitViewModel(_model);

            // Canvas
            _canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            // HealthBar (с Image)
            var healthBarGO = new GameObject("HealthBar");
            healthBarGO.transform.SetParent(_canvasGO.transform);
            _healthBar = healthBarGO.AddComponent<UnitHealthBar>();
            var img = healthBarGO.AddComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;

            // Тексты
            var amountGO = new GameObject("AmountText");
            amountGO.transform.SetParent(_canvasGO.transform);
            _amountText = amountGO.AddComponent<TextMeshProUGUI>();

            var healthAmountGO = new GameObject("HealthAmountText");
            healthAmountGO.transform.SetParent(_canvasGO.transform);
            _healthAmountText = healthAmountGO.AddComponent<TextMeshProUGUI>();

            // UnitViewUI и привязки приватных полей
            var uiGO = new GameObject("UnitViewUI");
            uiGO.transform.SetParent(_canvasGO.transform);
            _ui = uiGO.AddComponent<UnitViewUI>();

            var fiHealthBar = typeof(UnitViewUI).GetField("healthBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiHealthBar?.SetValue(_ui, _healthBar);

            var fiAmountText = typeof(UnitViewUI).GetField("amountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiAmountText?.SetValue(_ui, _amountText);

            var fiHealthAmountText = typeof(UnitViewUI).GetField("healthAmountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiHealthAmountText?.SetValue(_ui, _healthAmountText);

            // Инициализация
            _ui.Init(_vm);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_stats != null) UnityEngine.Object.DestroyImmediate(_stats);
            if (_canvasGO != null) UnityEngine.Object.DestroyImmediate(_canvasGO);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Damage_UpdatesHealthAmountText_And_HealthBarRatio()
        {
            // Act: наносим 40 урона → здоровье 60/100
            _model.RecieveDamage(new DamageContext(40, DamageType.physical, null));
            yield return new WaitForSeconds(0.05f);

            // Assert: текст и fillAmount
            Assert.That(_healthAmountText.text, Is.EqualTo("60"));
            Assert.That(_vm.HealthRatio, Is.EqualTo(0.6f));
        }

        [UnityTest]
        public IEnumerator LethalDamage_DecreasesAmount_ResetsHealthText_ToMax()
        {
            // Начальное количество 3
            var initialAmount = _model.Amount.Value;

            // Act: наносим ровно 100 урона — минус один юнит, след. юнит со 100 HP
            _model.RecieveDamage(new DamageContext(100, DamageType.physical, null));
            yield return new WaitForSeconds(0.05f);

            // Assert: количество уменьшилось, здоровье снова 100
            Assert.That(_model.Amount.Value, Is.EqualTo(initialAmount - 1));
            Assert.That(_healthAmountText.text, Is.EqualTo("100"));
            Assert.That(_vm.HealthRatio, Is.EqualTo(1.0f));
            Assert.That(_amountText.text, Is.EqualTo((initialAmount - 1).ToString()));
        }
    }
}
