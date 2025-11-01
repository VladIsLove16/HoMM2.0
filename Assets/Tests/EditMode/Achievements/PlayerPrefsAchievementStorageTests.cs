using System.Linq;
using Game.Achievements;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Achievements
{
    [TestFixture]
    public class PlayerPrefsAchievementStorageTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void Load_ReturnsEmpty_WhenNothingSaved()
        {
            var storage = new PlayerPrefsAchievementStorage();

            var loaded = storage.Load();

            Assert.That(loaded, Is.Empty);
        }

        [Test]
        public void Save_And_Load_PersistData()
        {
            var storage = new PlayerPrefsAchievementStorage();
            var expected = new[] { "ach1", "ach2" };

            storage.Save(expected);

            var loaded = storage.Load().ToArray();

            CollectionAssert.AreEquivalent(expected, loaded);
        }
    }
}
