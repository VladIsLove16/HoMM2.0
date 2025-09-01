using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace Tests.PlayMode.UI
{
    public class UnitHealthBarTests_PlayMode
    {
        private GameObject _gameObject;
        private UnitHealthBar _healthBar;
        private Image _healthImage;
        private TextMeshProUGUI _healthText;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Создаем GameObject с компонентами
            _gameObject = new GameObject("TestHealthBar");
            
            // Добавляем Canvas для UI элементов
            var canvas = _gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _gameObject.AddComponent<CanvasScaler>();
            _gameObject.AddComponent<GraphicRaycaster>();

            // Создаем Image для полоски здоровья
            var imageGameObject = new GameObject("HealthImage");
            imageGameObject.transform.SetParent(_gameObject.transform);
            _healthImage = imageGameObject.AddComponent<Image>();
            _healthImage.type = Image.Type.Filled;
            _healthImage.fillMethod = Image.FillMethod.Horizontal;

            // Создаем Text для отображения здоровья
            var textGameObject = new GameObject("HealthText");
            textGameObject.transform.SetParent(_gameObject.transform);
            _healthText = textGameObject.AddComponent<TextMeshProUGUI>();

            // Добавляем UnitHealthBar
            _healthBar = _gameObject.AddComponent<UnitHealthBar>();
            
            // Устанавливаем ссылки через reflection для тестов
            var healthTextField = typeof(UnitHealthBar).GetField("HealthText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthTextField?.SetValue(_healthBar, _healthText);

            var healthImageField = typeof(UnitHealthBar).GetField("HealthImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthImageField?.SetValue(_healthBar, _healthImage);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Init_SetsHealthBarToFullHealth()
        {
            // Act
            _healthBar.Init();

            yield return null;

            // Assert
            Assert.That(_healthImage.fillAmount, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator SetRatio_WithValidRatio_UpdatesHealthBarCorrectly()
        {
            // Arrange
            float testRatio = 0.75f;

            // Act
            _healthBar.SetRatio(testRatio);

            yield return null;

            // Assert
            Assert.That(_healthImage.fillAmount, Is.EqualTo(testRatio));
        }

        [UnityTest]
        public IEnumerator SetRatio_WithZeroRatio_SetsHealthBarToEmpty()
        {
            // Act
            _healthBar.SetRatio(0f);

            yield return null;

            // Assert
            Assert.That(_healthImage.fillAmount, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator SetRatio_WithFullRatio_SetsHealthBarToFull()
        {
            // Act
            _healthBar.SetRatio(1f);

            yield return null;

            // Assert
            Assert.That(_healthImage.fillAmount, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator HideOnFullHP_WhenTrueAndRatioIsOne_HidesHealthBar()
        {
            // Arrange
            var hideOnFullHPField = typeof(UnitHealthBar).GetField("HideOnFullHP", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hideOnFullHPField?.SetValue(_healthBar, true);

            // Act
            _healthBar.SetRatio(1f);

            yield return null;

            // Assert
            Assert.That(_healthImage.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator HideOnFullHP_WhenTrueAndRatioIsNotOne_ShowsHealthBar()
        {
            // Arrange
            var hideOnFullHPField = typeof(UnitHealthBar).GetField("HideOnFullHP", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hideOnFullHPField?.SetValue(_healthBar, true);

            // Act
            _healthBar.SetRatio(0.5f);

            yield return null;

            // Assert
            Assert.That(_healthImage.gameObject.activeSelf, Is.True);
        }
    }
}
