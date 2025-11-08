using Game.Events;
using UniRx;
using System;
using System.Collections.Generic;
using CustomEventBus;

namespace Tests.EditMode.MushroomBook
{
    public sealed class StubAchievementEventBus : EventBus
    {
        public List<MushroomCollectedCustomEvent> Collected { get; } = new();
        public List<BattleCompletedCustomEvent> Battles { get; } = new();

        public IObservable<MushroomCollectedCustomEvent> MushroomCollectedStream => Observable.Empty<MushroomCollectedCustomEvent>();
        public IObservable<BattleCompletedCustomEvent> BattleCompletedStream => Observable.Empty<BattleCompletedCustomEvent>();
    }
}







