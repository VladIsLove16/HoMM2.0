using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tests.EditMode.UI
{
    [TestFixture]
    public class UnitHealthBar_EditModeTests
    {
        private GameObject _root;
        private UnitHealthBar _healthBar;
        private Image _healthImage;
        private TextMeshProUGUI _healthText;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestHealthBar");
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _root.AddComponent<CanvasScaler>();
            _root.AddComponent<GraphicRaycaster>();

            var imageGO = new GameObject("HealthImage");
            imageGO.transform.SetParent(_root.transform);
            _healthImage = imageGO.AddComponent<Image>();
            _healthImage.type = Image.Type.Filled;
            _healthImage.fillMethod = Image.FillMethod.Horizontal;

            var textGO = new GameObject("HealthText");
            textGO.transform.SetParent(_root.transform);
            _healthText = textGO.AddComponent<TextMeshProUGUI>();

            _healthBar = _root.AddComponent<UnitHealthBar>();

            typeof(UnitHealthBar).GetField("HealthText", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_healthBar, _healthText);
            typeof(UnitHealthBar).GetField("HealthImage", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_healthBar, _healthImage);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }
        }

        [Test]
        public void Init_SetsHealthBarToFullHealth()
        {
            _healthBar.Init();

            Assert.That(_healthImage.fillAmount, Is.EqualTo(1f));
        }

        [Test]
        public void SetRatio_UpdatesHealthBarFill()
        {
            _healthBar.SetRatio(0.75f);

            Assert.That(_healthImage.fillAmount, Is.EqualTo(0.75f));
        }

        [Test]
        public void SetRatio_ToZero_ClearsHealthBar()
        {
            _healthBar.SetRatio(0f);

            Assert.That(_healthImage.fillAmount, Is.EqualTo(0f));
        }

        [Test]
        public void SetRatio_ToOne_FillsHealthBar()
        {
            _healthBar.SetRatio(1f);

            Assert.That(_healthImage.fillAmount, Is.EqualTo(1f));
        }

        [Test]
        public void HideOnFullHP_DisablesImage_WhenRatioIsOne()
        {
            typeof(UnitHealthBar).GetField("HideOnFullHP", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_healthBar, true);

            _healthBar.SetRatio(1f);

            Assert.That(_healthImage.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void HideOnFullHP_EnablesImage_WhenRatioBelowOne()
        {
            typeof(UnitHealthBar).GetField("HideOnFullHP", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_healthBar, true);

            _healthBar.SetRatio(0.5f);

            Assert.That(_healthImage.gameObject.activeSelf, Is.True);
        }
    }
}
