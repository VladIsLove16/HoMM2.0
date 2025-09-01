using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class SimpleEditModeTest
    {
        [Test]
        public void SimpleTest_ShouldBeEditMode()
        {
            // Arrange
            var testValue = 42;

            // Act & Assert
            Assert.That(testValue, Is.EqualTo(42));
        }

        [Test]
        public void AnotherTest_ShouldAlsoBeEditMode()
        {
            // Arrange
            var testString = "Hello World";

            // Act & Assert
            Assert.That(testString, Is.EqualTo("Hello World"));
        }
    }
}
