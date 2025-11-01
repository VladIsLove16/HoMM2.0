using Game.Achievements;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests.EditMode.Achievements
{
    [TestFixture]
    public class AchievementServiceTests
    {
        [Test]
        public void TryUnlock_AddsAchievement_WhenDefinitionExists()
        {
            var definitions = new[] { CreateDefinition(AchievementIds.FirstMushroom) };
            var storage = new InMemoryAchievementStorage();
            var service = new AchievementService(new InMemoryDefinitionProvider(definitions), storage);

            var unlocked = service.TryUnlock(AchievementIds.FirstMushroom);

            Assert.That(unlocked, Is.True);
            Assert.That(service.IsUnlocked(AchievementIds.FirstMushroom), Is.True);
            CollectionAssert.Contains(storage.LastSaved, AchievementIds.FirstMushroom);
        }

        [Test]
        public void TryUnlock_ReturnsFalse_WhenAlreadyUnlocked()
        {
            var definitions = new[] { CreateDefinition(AchievementIds.FirstMushroom) };
            var storage = new InMemoryAchievementStorage(new[] { AchievementIds.FirstMushroom });
            var service = new AchievementService(new InMemoryDefinitionProvider(definitions), storage);

            var unlocked = service.TryUnlock(AchievementIds.FirstMushroom);

            Assert.That(unlocked, Is.False);
            Assert.That(storage.SaveCallCount, Is.EqualTo(0));
        }

        [Test]
        public void TryUnlock_ReturnsFalse_WhenUnknownId()
        {
            var definitions = new[] { CreateDefinition(AchievementIds.FirstMushroom) };
            var storage = new InMemoryAchievementStorage();
            var service = new AchievementService(new InMemoryDefinitionProvider(definitions), storage);

            var unlocked = service.TryUnlock("unknown_id");

            Assert.That(unlocked, Is.False);
            Assert.That(storage.SaveCallCount, Is.EqualTo(0));
        }

        private static AchievementDefinition CreateDefinition(string id)
        {
            return AchievementDefinitionBuilder.New().WithId(id).WithTitle(id).Create();
        }

        private sealed class InMemoryDefinitionProvider : IAchievementDefinitionProvider
        {
            private readonly Dictionary<string, AchievementDefinition> _lookup;
            private readonly IReadOnlyList<AchievementDefinition> _all;

            public InMemoryDefinitionProvider(IEnumerable<AchievementDefinition> definitions)
            {
                _all = new List<AchievementDefinition>(definitions);
                _lookup = new Dictionary<string, AchievementDefinition>(StringComparer.OrdinalIgnoreCase);
                foreach (var definition in _all)
                {
                    _lookup[definition.Id] = definition;
                }
            }

            public IReadOnlyList<AchievementDefinition> All => _all;

            public bool TryGet(string id, out AchievementDefinition definition) => _lookup.TryGetValue(id, out definition);
        }

        private sealed class InMemoryAchievementStorage : IAchievementStorage
        {
            private readonly HashSet<string> _initial;

            public InMemoryAchievementStorage(IEnumerable<string> initial = null)
            {
                _initial = initial != null
                    ? new HashSet<string>(initial, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public int SaveCallCount { get; private set; }
            public IReadOnlyCollection<string> LastSaved { get; private set; } = Array.Empty<string>();

            public IEnumerable<string> Load() => _initial;

            public void Save(IEnumerable<string> unlockedIds)
            {
                SaveCallCount++;
                LastSaved = new List<string>(unlockedIds);
            }
        }
    }
}
