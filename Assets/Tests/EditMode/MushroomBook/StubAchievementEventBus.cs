using Game.Events;
using UniRx;
using System;
using System.Collections.Generic;

namespace Tests.EditMode.MushroomBook
{
    public partial class MushroomBookViewTests
    {
        private sealed class StubAchievementEventBus : IGameplayEventBus
        {
            public List<MushroomCollectedEvent> Collected { get; } = new();
            public List<BattleCompletedEvent> Battles { get; } = new();

            public IObservable<MushroomCollectedEvent> MushroomCollectedStream => Observable.Empty<MushroomCollectedEvent>();
            public IObservable<BattleCompletedEvent> BattleCompletedStream => Observable.Empty<BattleCompletedEvent>();

            public void PublishMushroomCollected(string itemId, IReadOnlyDictionary<string, int> totals)
            {
                Collected.Add(new MushroomCollectedEvent(itemId, totals));
            }

            public void PublishBattleCompleted(bool playerWon)
            {
                Battles.Add(new BattleCompletedEvent(playerWon));
            }
        }
    }
}







