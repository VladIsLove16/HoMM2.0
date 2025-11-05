using System;
using System.Collections.Generic;
using UniRx;

namespace Game.Events
{
    public sealed class GameplayEventBus : IGameplayEventBus
    {
        private readonly Subject<MushroomCollectedEvent> _mushroomCollected = new();
        private readonly Subject<BattleCompletedEvent> _battleCompleted = new();

        public IObservable<MushroomCollectedEvent> MushroomCollectedStream => _mushroomCollected;
        public IObservable<BattleCompletedEvent> BattleCompletedStream => _battleCompleted;

        public void PublishMushroomCollected(string itemId, IReadOnlyDictionary<string, int> totals)
        {
            _mushroomCollected.OnNext(new MushroomCollectedEvent(itemId, totals));
        }

        public void PublishBattleCompleted(bool playerWon)
        {
            _battleCompleted.OnNext(new BattleCompletedEvent(playerWon));
        }
    }
}
