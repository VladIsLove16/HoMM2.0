using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tests.Common;

namespace Tests.EditMode.UI
{
    [TestFixture]
    public class UnitHealthBar_EditModeIntegrationTests
    {
        private GameObject _canvasRoot;
        private GameObject _cameraGO;
        private UnitHealthBar _healthBar;
        private TextMeshProUGUI _amountText;
        private TextMeshProUGUI _healthAmountText;
        private UnitViewUI _unitViewUI;
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
        private UnitStats _stats;

        [SetUp]
        public void SetUp()
        {
            _cameraGO = new GameObject("MainCamera");
            _cameraGO.tag = "MainCamera";
            _cameraGO.AddComponent<Camera>();

            _canvasRoot = new GameObject("UnitViewUI_Test", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var healthBarGO = new GameObject("HealthBar");
            healthBarGO.transform.SetParent(_canvasRoot.transform);
            _healthBar = healthBarGO.AddComponent<UnitHealthBar>();

            var healthImageGO = new GameObject("HealthImage");
            healthImageGO.transform.SetParent(healthBarGO.transform);
            var healthImage = healthImageGO.AddComponent<Image>();
            healthImage.type = Image.Type.Filled;
            healthImage.fillMethod = Image.FillMethod.Horizontal;

            var healthTextGO = new GameObject("HealthText");
            healthTextGO.transform.SetParent(healthBarGO.transform);
            var healthText = healthTextGO.AddComponent<TextMeshProUGUI>();

            typeof(UnitHealthBar).GetField("HealthImage", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_healthBar, healthImage);
            typeof(UnitHealthBar).GetField("HealthText", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_healthBar, healthText);

            var amountTextGO = new GameObject("AmountText");
            amountTextGO.transform.SetParent(_canvasRoot.transform);
            _amountText = amountTextGO.AddComponent<TextMeshProUGUI>();

            var healthAmountGO = new GameObject("HealthAmountText");
            healthAmountGO.transform.SetParent(_canvasRoot.transform);
            _healthAmountText = healthAmountGO.AddComponent<TextMeshProUGUI>();

            _unitViewUI = _canvasRoot.AddComponent<UnitViewUI>();
            typeof(UnitViewUI).GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_unitViewUI, _healthBar);
            typeof(UnitViewUI).GetField("amountText", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_unitViewUI, _amountText);
            typeof(UnitViewUI).GetField("healthAmountText", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_unitViewUI, _healthAmountText);

            _stats = ScriptableObject.CreateInstance<UnitStats>();
            _stats.Health = 100;
            _stats.MaxHealth = 100;
            _stats.Damage = 25;
            _stats.InvulnerableEffects = new System.Collections.Generic.List<StatusEffectType>();

            _unitModel = new UnitModel(_stats, UnitType.Archer, 0, 0, 10, true);
            _unitViewModel = new UnitViewModel(_unitModel, new TestWorldToCellProvider());
        }

        [TearDown]
        public void TearDown()
        {
            _unitViewUI?.Dispose();
            if (_canvasRoot != null) Object.DestroyImmediate(_canvasRoot);
            if (_cameraGO != null) Object.DestroyImmediate(_cameraGO);
            if (_stats != null) Object.DestroyImmediate(_stats);
        }

        [Test]
        public void Init_BindsInitialValues()
        {
            _unitViewUI.Init(_unitViewModel);

            Assert.That(_amountText.text, Is.EqualTo("10"));
            Assert.That(_healthAmountText.text, Is.EqualTo("100/100"));
            Assert.That(_healthBar.GetComponentInChildren<Image>().fillAmount, Is.EqualTo(1f));
        }

        [Test]
        public void RecieveDamage_UpdatesHealthBarAndTexts()
        {
            _unitViewUI.Init(_unitViewModel);

            _unitModel.RecieveDamage(new DamageContext(30, DamageType.physical, null));

            Assert.That(_healthAmountText.text, Is.EqualTo("70/100"));
            Assert.That(_amountText.text, Is.EqualTo("10"));
            Assert.That(_healthBar.GetComponentInChildren<Image>().fillAmount, Is.EqualTo(0.7f).Within(0.0001f));
        }

        [Test]
        public void LethalDamage_UpdatesAmountAndColor()
        {
            _unitViewUI.Init(_unitViewModel);

            _unitModel.RecieveDamage(new DamageContext(1000, DamageType.physical, null));

            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0));
            Assert.That(_amountText.color, Is.EqualTo(Color.black));
        }
    }
}
