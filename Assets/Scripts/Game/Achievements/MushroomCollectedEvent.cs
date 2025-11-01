using System.Collections.Generic;

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
}
