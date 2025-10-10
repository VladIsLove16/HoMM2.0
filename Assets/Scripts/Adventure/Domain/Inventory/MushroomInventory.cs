using System.Collections.Generic;

namespace Adventure.Domain.Inventory
{
    public sealed class MushroomInventory
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int> Items => _items;

        public void Add(string mushroomId, int amount = 1)
        {
            if (string.IsNullOrEmpty(mushroomId) || amount <= 0)
                return;

            if (_items.TryGetValue(mushroomId, out var existing))
            {
                _items[mushroomId] = existing + amount;
            }
            else
            {
                _items[mushroomId] = amount;
            }
        }

        public int GetCount(string mushroomId)
        {
            return _items.TryGetValue(mushroomId, out var value) ? value : 0;
        }
    }
}
