using System;
using System.Collections.Generic;
using System.Linq;
using CustomEventBus;
using Game.Achievements;
using Game.Events;
using NUnit.Framework;
using UniRx;

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
                CreateDefinition(AchievementIds.FirstMushroom),
                CreateDefinition(AchievementIds.MushroomCollector),
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
                CreateDefinition(AchievementIds.FirstBattleWin)
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
                _lookup = _all.ToDictionary(def => def.Id, def => def, StringComparer.OrdinalIgnoreCase);
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

            public IEnumerable<string> Load() => _initial;

            public void Save(IEnumerable<string> unlockedIds)
            {
                _initial.Clear();
                foreach (var id in unlockedIds)
                {
                    _initial.Add(id);
                }
            }
        }
    }
}


