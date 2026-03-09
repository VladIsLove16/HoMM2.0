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

            Assert.That(loaded.Entries, Is.Empty);
        }

        [Test]
        public void Save_And_Load_PersistData()
        {
            var storage = new PlayerPrefsAchievementStorage();
            var data = new AchievementProgressStorageData();
            data.Entries.Add(new AchievementProgressSnapshot { Id = "ach1", IsUnlocked = true });
            data.Entries.Add(new AchievementProgressSnapshot { Id = "ach2", IsUnlocked = true });

            storage.Save(data);

            var loaded = storage.Load();

            var ids = loaded.Entries.Select(entry => entry.Id).ToArray();
            CollectionAssert.AreEquivalent(new[] { "ach1", "ach2" }, ids);
        }
    }
}
