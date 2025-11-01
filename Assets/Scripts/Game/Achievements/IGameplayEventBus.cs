using System;
using System.Collections.Generic;

namespace Adventure.Infrastructure.Events
{
    public interface IGameplayEventBus
    {
        IObservable<MushroomCollectedEvent> MushroomCollectedStream { get; }
        IObservable<BattleCompletedEvent> BattleCompletedStream { get; }

        void PublishMushroomCollected(UnitType type, IReadOnlyDictionary<UnitType, int> totals);
        void PublishBattleCompleted(bool playerWon);
    }
}
