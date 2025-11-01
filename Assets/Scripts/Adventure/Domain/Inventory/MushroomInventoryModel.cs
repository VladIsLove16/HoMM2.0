using Adventure.Integration.Battle;
using Adventure.Infrastructure.State;
using System.Collections.Generic;

namespace Adventure.Domain.Inventory
{
    public sealed class MushroomInventoryModel
    {
        private readonly Dictionary<UnitType, int> _items = new Dictionary<UnitType, int>();

        public IReadOnlyDictionary<UnitType, int> Items => _items;
        public MushroomInventoryModel(IReadOnlyList<UnitStackData> unitStackDatas)
        {
            if (AdventureStateCache.TryGetInventorySnapshot(out var cachedSnapshot) && cachedSnapshot != null && cachedSnapshot.Count > 0)
            {
                foreach (var item in cachedSnapshot)
                {
                    AddInternal(item.UnitType, item.Amount, false);
                }
            }
            else if (unitStackDatas != null)
            {
                foreach (var item in unitStackDatas)
                {
                    AddInternal(item.UnitType, item.Amount, false);
                }
                AdventureStateCache.StoreInventorySnapshot(GetData());
            }
        }
        public void Add(UnitType mushroomId, int amount = 1)
        {
            AddInternal(mushroomId, amount, true);
        }

        private void AddInternal(UnitType mushroomId, int amount, bool updateSnapshot)
        {
            if (amount <= 0)
                return;

            if (_items.TryGetValue(mushroomId, out var existing))
            {
                _items[mushroomId] = existing + amount;
            }
            else
            {
                _items[mushroomId] = amount;
            }

            if (updateSnapshot)
            {
                AdventureStateCache.StoreInventorySnapshot(GetData());
            }
        }
        public  IReadOnlyList<UnitStackData> GetData()
        {
            List<UnitStackData> unitStackDatas = new List<UnitStackData>();
            foreach (var item in _items)
            {
                unitStackDatas.Add(new(item.Key, item.Value));
            }
            return unitStackDatas;
        }
        public int GetAmount(UnitType mushroomId)
        {
            return _items.TryGetValue(mushroomId, out var value) ? value : 0;
        }
    }
}
