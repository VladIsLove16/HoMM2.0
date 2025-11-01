using System;
using System.Collections.Generic;
using Adventure.Integration.Battle;

namespace Adventure.Infrastructure.Events
{
    public readonly struct MushroomCollectedEvent
    {
        public MushroomCollectedEvent(UnitType type, IReadOnlyDictionary<UnitType, int> totals)
        {
            Type = type;
            Totals = totals;
        }

        public UnitType Type { get; }
        public IReadOnlyDictionary<UnitType, int> Totals { get; }
    }

    public readonly struct BattleCompletedEvent
    {
        public BattleCompletedEvent(bool playerWon)
        {
            PlayerWon = playerWon;
        }

        public bool PlayerWon { get; }
    }

    public interface IGameplayEventBus
    {
        IObservable<MushroomCollectedEvent> MushroomCollectedStream { get; }
        IObservable<BattleCompletedEvent> BattleCompletedStream { get; }

        void PublishMushroomCollected(UnitType type, IReadOnlyDictionary<UnitType, int> totals);
        void PublishBattleCompleted(bool playerWon);
    }
}
