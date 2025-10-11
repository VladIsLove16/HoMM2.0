using System.Collections.Generic;

namespace Adventure.Domain.Inventory
{
    public sealed class MushroomInventoryModel
    {
        private readonly Dictionary<UnitType, int> _items = new Dictionary<UnitType, int>();

        public IReadOnlyDictionary<UnitType, int> Items => _items;

        public void Add(UnitType mushroomId, int amount = 1)
        {

            if (_items.TryGetValue(mushroomId, out var existing))
            {
                _items[mushroomId] = existing + amount;
            }
            else
            {
                _items[mushroomId] = amount;
            }
        }

        public int GetCount(UnitType mushroomId)
        {
            return _items.TryGetValue(mushroomId, out var value) ? value : 0;
        }
    }
}
