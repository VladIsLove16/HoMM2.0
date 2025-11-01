using System;
using System.Collections.Generic;
using UniRx;

namespace Adventure.Infrastructure.Events
{
    public sealed class GameplayEventBus : IGameplayEventBus
    {
        private readonly Subject<MushroomCollectedEvent> _mushroomCollected = new();
        private readonly Subject<BattleCompletedEvent> _battleCompleted = new();

        public IObservable<MushroomCollectedEvent> MushroomCollectedStream => _mushroomCollected;
        public IObservable<BattleCompletedEvent> BattleCompletedStream => _battleCompleted;

        public void PublishMushroomCollected(UnitType type, IReadOnlyDictionary<UnitType, int> totals)
        {
            _mushroomCollected.OnNext(new MushroomCollectedEvent(type, totals));
        }

        public void PublishBattleCompleted(bool playerWon)
        {
            _battleCompleted.OnNext(new BattleCompletedEvent(playerWon));
        }
    }
}
