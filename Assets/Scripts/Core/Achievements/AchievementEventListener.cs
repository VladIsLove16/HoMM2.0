using CustomEventBus;
using Game.Events;
using System;
using System.Linq;
using UniRx;
using Zenject;

namespace Game.Achievements
{
    public sealed class AchievementEventListener : IInitializable, IDisposable
    {
        private readonly EventBus _events;
        private readonly IAchievementService _service;
        private readonly CompositeDisposable _subscriptions = new();

        public AchievementEventListener(EventBus events, IAchievementService service)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Initialize()
        {
            _subscriptions.Add(_events.Subscribe<MushroomCollectedCustomEvent>(OnMushroomCollected));
            _subscriptions.Add(_events.Subscribe<BattleCompletedCustomEvent>(OnBattleCompleted));
        }

        private void OnMushroomCollected(MushroomCollectedCustomEvent data)
        {
            if (data.Totals == null)
                return;

            var totalCollected = data.Totals.Sum(pair => pair.Value);

            if (totalCollected >= 1)
            {
                _service.TryUnlock(AchievementIds.FirstMushroom);
            }

            if (totalCollected >= 10)
            {
                _service.TryUnlock(AchievementIds.MushroomCollector);
            }
        }

        private void OnBattleCompleted(BattleCompletedCustomEvent data)
        {
            if (data.PlayerWon)
            {
                _service.TryUnlock(AchievementIds.FirstBattleWin);
            }
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}
