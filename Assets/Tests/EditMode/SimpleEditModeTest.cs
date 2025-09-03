using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

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

        [UnityTest]
        public IEnumerator AnotherTest_ShouldAlsoBePlayMode()
        {
            // Arrange
            var testString = "Hello World";

            // Act & Assert
            yield return new WaitForSeconds(0.1f);  
            Assert.That(testString, Is.EqualTo("Hello World"));
        }
    }
}
