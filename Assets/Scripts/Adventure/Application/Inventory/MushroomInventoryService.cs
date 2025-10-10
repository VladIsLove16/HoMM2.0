using System.Collections.Generic;
using Adventure.Domain.Inventory;
using UniRx;

namespace Adventure.Application.Inventory
{
    public sealed class MushroomInventoryService
    {
        private readonly MushroomInventory _inventory;
        private readonly IMushroomCatalog _catalog;
        private readonly ReactiveCollection<MushroomInventoryEntry> _entries = new ReactiveCollection<MushroomInventoryEntry>();
        private readonly Dictionary<string, int> _indexById = new Dictionary<string, int>();

        public MushroomInventoryService(MushroomInventory inventory, IMushroomCatalog catalog)
        {
            _inventory = inventory;
            _catalog = catalog;
            RebuildEntries();
        }

        public IReadOnlyReactiveCollection<MushroomInventoryEntry> Entries => _entries;

        public void Add(string mushroomId, int amount = 1)
        {
            if (string.IsNullOrEmpty(mushroomId) || amount <= 0)
                return;

            _inventory.Add(mushroomId, amount);
            var count = _inventory.GetCount(mushroomId);
            if (_catalog.TryGet(mushroomId, out var item))
            {
                var entry = new MushroomInventoryEntry(item, count);
                if (_indexById.TryGetValue(mushroomId, out var index))
                {
                    _entries[index] = entry;
                }
                else
                {
                    _indexById[mushroomId] = _entries.Count;
                    _entries.Add(entry);
                }
            }
        }

        private void RebuildEntries()
        {
            _entries.Clear();
            _indexById.Clear();
            foreach (var kvp in _inventory.Items)
            {
                if (_catalog.TryGet(kvp.Key, out var item))
                {
                    var index = _entries.Count;
                    _indexById[kvp.Key] = index;
                    _entries.Add(new MushroomInventoryEntry(item, kvp.Value));
                }
            }
        }
    }

    public readonly struct MushroomInventoryEntry
    {
        public MushroomInventoryEntry(MushroomItem item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        public MushroomItem Item { get; }
        public int Amount { get; }
    }
}
