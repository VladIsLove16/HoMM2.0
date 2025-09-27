using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class SimplePlayModeTest
    {
        [UnityTest]
        public IEnumerator SimpleTest_ShouldBePlayMode()
        {
            // Arrange
            var testValue = 42;

            // Act & Assert
            Assert.That(testValue, Is.EqualTo(42));

            yield return null;
        }

        [UnityTest]
        public IEnumerator AnotherTest_ShouldAlsoBePlayMode()
        {
            // Arrange
            var testString = "Hello World";

            // Act & Assert
            Assert.That(testString, Is.EqualTo("Hello World"));

            yield return null;
        }
    }
}
