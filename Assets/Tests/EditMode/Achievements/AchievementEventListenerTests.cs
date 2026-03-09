using System;
using System.Collections.Generic;
using System.Linq;
using CustomEventBus;
using Game.Achievements;
using Game.Events;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Tests.EditMode.Achievements
{
    [TestFixture]
    public class AchievementEventListenerTests
    {
        [Test]
        public void UnlocksMushroomMilestones_WhenThresholdsMet()
        {
            var definitions = new[]
            {
                CreateDefinition(AchievementIds.FirstMushroom, CreateCounterCondition(1)),
                CreateDefinition(AchievementIds.MushroomCollector, CreateCounterCondition(10)),
            };

            var storage = new InMemoryAchievementStorage();
            var service = new AchievementService(new InMemoryDefinitionProvider(definitions), storage);
            var bus = new EventBus();
            var listener = new AchievementEventListener(bus, service);
            listener.Initialize();

            bus.Invoke(new MushroomCollectedCustomEvent( "Witch", new Dictionary<string, int> { { "Witch", 1 } }));
            Assert.That(service.IsUnlocked(AchievementIds.FirstMushroom), Is.True);

            bus.Invoke((new MushroomCollectedCustomEvent("Witch", new Dictionary<string, int> { { "Witch", 9 }, { "Archer", 1 } })));
            Assert.That(service.IsUnlocked(AchievementIds.MushroomCollector), Is.True);

            listener.Dispose();
        }

        [Test]
        public void UnlocksBattleAchievement_OnVictory()
        {
            var definitions = new[]
            {
                CreateDefinition(AchievementIds.FirstBattleWin, CreateEventCondition(AchievementEventIds.BattleWon))
            };

            var storage = new InMemoryAchievementStorage();
            var service = new AchievementService(new InMemoryDefinitionProvider(definitions), storage);
            var bus = new EventBus();
            var listener = new AchievementEventListener(bus, service);
            listener.Initialize();

            bus.Invoke(new BattleCompletedCustomEvent( true));
            Assert.That(service.IsUnlocked(AchievementIds.FirstBattleWin), Is.True);

            listener.Dispose();
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

        private static AchievementCounterConditionSO CreateCounterCondition(int target)
        {
            var condition = ScriptableObject.CreateInstance<AchievementCounterConditionSO>();
            typeof(AchievementCounterConditionSO)
                .GetField("eventId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(condition, AchievementEventIds.MushroomCollectedTotal);
            typeof(AchievementCounterConditionSO)
                .GetField("targetValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(condition, target);
            typeof(AchievementCounterConditionSO)
                .GetField("useMaxValueFromEvent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(condition, true);
            return condition;
        }

        private sealed class InMemoryDefinitionProvider : IAchievementDefinitionProvider
        {
            private readonly Dictionary<string, AchievementDefinition> _lookup;
            private readonly IReadOnlyList<AchievementDefinition> _all;

            public InMemoryDefinitionProvider(IEnumerable<AchievementDefinition> definitions)
            {
                _all = new List<AchievementDefinition>(definitions);
                _lookup = _all.ToDictionary(def => def.Id, def => def, StringComparer.OrdinalIgnoreCase);
            }

            public IReadOnlyList<AchievementDefinition> All => _all;

            public bool TryGet(string id, out AchievementDefinition definition) => _lookup.TryGetValue(id, out definition);
        }

        private sealed class InMemoryAchievementStorage : IAchievementStorage
        {
            public AchievementProgressStorageData Load() => new AchievementProgressStorageData();

            public void Save(AchievementProgressStorageData data)
            {
            }

            public void Clear()
            {
            }
        }
    }
}


