using System;
using System.Collections.Generic;

namespace Game.Events
{
    public interface IGameplayEventBus
    {
        IObservable<MushroomCollectedEvent> MushroomCollectedStream { get; }
        IObservable<BattleCompletedEvent> BattleCompletedStream { get; }

        void PublishMushroomCollected(string itemId, IReadOnlyDictionary<string, int> totals);
        void PublishBattleCompleted(bool playerWon);
    }
}
