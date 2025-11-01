using System;
using System.Linq;
using Adventure.Infrastructure.Events;
using UniRx;
using Zenject;

namespace Game.Achievements
{
    public sealed class AchievementEventListener : IInitializable, IDisposable
    {
        private readonly IGameplayEventBus _events;
        private readonly IAchievementService _service;
        private readonly CompositeDisposable _subscriptions = new();

        public AchievementEventListener(IGameplayEventBus events, IAchievementService service)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Initialize()
        {
            _events.MushroomCollectedStream
                .Subscribe(OnMushroomCollected)
                .AddTo(_subscriptions);

            _events.BattleCompletedStream
                .Subscribe(OnBattleCompleted)
                .AddTo(_subscriptions);
        }

        private void OnMushroomCollected(MushroomCollectedEvent data)
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

        private void OnBattleCompleted(BattleCompletedEvent data)
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
