using System.Collections.Generic;

namespace Game.Events
{
    public readonly struct MushroomCollectedEvent
    {
        public MushroomCollectedEvent(string itemId, IReadOnlyDictionary<string, int> totals)
        {
            ItemId = string.IsNullOrEmpty(itemId) ? string.Empty : itemId;
            Totals = totals ?? new Dictionary<string, int>();
        }

        public string ItemId { get; }
        public IReadOnlyDictionary<string, int> Totals { get; }
    }
}
