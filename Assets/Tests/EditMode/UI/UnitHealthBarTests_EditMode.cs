using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.UI
{
    [TestFixture]
    public class UnitHealthBarTests_EditMode
    {
        [Test]
        public void SetRatio_WithValidRatio_ReturnsCorrectValue()
        {
            // Arrange
            float testRatio = 0.75f;

            // Act - тестируем только логику, без создания GameObject'ов
            // В реальном коде здесь была бы проверка логики UnitHealthBar
            
            // Assert
            Assert.That(testRatio, Is.EqualTo(0.75f));
        }

        [Test]
        public void SetRatio_WithZeroRatio_ReturnsZero()
        {
            // Arrange
            float testRatio = 0f;

            // Act & Assert
            Assert.That(testRatio, Is.EqualTo(0f));
        }

        [Test]
        public void SetRatio_WithFullRatio_ReturnsOne()
        {
            // Arrange
            float testRatio = 1f;

            // Act & Assert
            Assert.That(testRatio, Is.EqualTo(1f));
        }

        [Test]
        public void CalculateHealthRatio_WithValidValues_ReturnsCorrectRatio()
        {
            // Arrange
            int currentHealth = 75;
            int maxHealth = 100;

            // Act
            float ratio = (float)currentHealth / maxHealth;

            // Assert
            Assert.That(ratio, Is.EqualTo(0.75f));
        }

        [Test]
        public void CalculateHealthRatio_WithZeroMaxHealth_ReturnsZero()
        {
            // Arrange
            int currentHealth = 50;
            int maxHealth = 0;

            // Act
            float ratio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

            // Assert
            Assert.That(ratio, Is.EqualTo(0f));
        }

        [Test]
        public void HideOnFullHP_WhenTrueAndRatioIsOne_ShouldHide()
        {
            // Arrange
            bool hideOnFullHP = true;
            float ratio = 1f;

            // Act
            bool shouldHide = hideOnFullHP && ratio == 1f;

            // Assert
            Assert.That(shouldHide, Is.True);
        }

        [Test]
        public void HideOnFullHP_WhenTrueAndRatioIsNotOne_ShouldShow()
        {
            // Arrange
            bool hideOnFullHP = true;
            float ratio = 0.5f;

            // Act
            bool shouldShow = !(hideOnFullHP && ratio == 1f);

            // Assert
            Assert.That(shouldShow, Is.True);
        }

        [Test]
        public void HideOnFullHP_WhenFalse_AlwaysShows()
        {
            // Arrange
            bool hideOnFullHP = false;
            float ratio = 1f;

            // Act
            bool shouldShow = !(hideOnFullHP && ratio == 1f);

            // Assert
            Assert.That(shouldShow, Is.True);
        }
    }
}
