using Game.Achievements;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.Achievements
{
    [TestFixture]
    public class AchievementServiceTests
    {
        [Test]
        public void HandleEvent_UnlocksAchievement_WhenConditionMet()
        {
            var condition = CreateEventCondition(AchievementEventIds.BattleWon);
            var definitions = new[] { CreateDefinition(AchievementIds.FirstBattleWin, condition) };
            var storage = new InMemoryAchievementStorage();
            var service = new AchievementService(new InMemoryDefinitionProvider(definitions), storage);

            service.HandleEvent(new AchievementEvent(AchievementEventIds.BattleWon, 1));

            Assert.That(service.IsUnlocked(AchievementIds.FirstBattleWin), Is.True);
            Assert.That(storage.SaveCallCount, Is.GreaterThan(0));
        }

        [Test]
        public void HandleEvent_DoesNotUnlock_WhenEventDoesNotMatch()
        {
            var condition = CreateEventCondition(AchievementEventIds.BattleWon);
            var definitions = new[] { CreateDefinition(AchievementIds.FirstBattleWin, condition) };
            var storage = new InMemoryAchievementStorage();
            var service = new AchievementService(new InMemoryDefinitionProvider(definitions), storage);

            service.HandleEvent(new AchievementEvent("other_event", 1));

            Assert.That(service.IsUnlocked(AchievementIds.FirstBattleWin), Is.False);
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

        private static AchievementDefinition CreateDefinition(string id, params AchievementConditionSO[] conditions)
        {
            return AchievementDefinitionBuilder.New().WithId(id).WithTitle(id).WithConditions(conditions).Create();
        }

        private static AchievementEventConditionSO CreateEventCondition(string eventId)
        {
            var condition = ScriptableObject.CreateInstance<AchievementEventConditionSO>();
            typeof(AchievementEventConditionSO)
                .GetField("eventId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(condition, eventId);
            return condition;
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
            public int SaveCallCount { get; private set; }
            public AchievementProgressStorageData LastSaved { get; private set; }

            public AchievementProgressStorageData Load() => new AchievementProgressStorageData();

            public void Save(AchievementProgressStorageData data)
            {
                SaveCallCount++;
                LastSaved = data;
            }

            public void Clear()
            {
                SaveCallCount++;
                LastSaved = new AchievementProgressStorageData();
            }
        }
    }
}
