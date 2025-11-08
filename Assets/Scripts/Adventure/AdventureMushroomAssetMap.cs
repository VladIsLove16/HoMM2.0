using System;
using System.Collections.Generic;
using System.Linq;
using Adventure.Infrastructure.Inventory;
using UnityEngine;

[CreateAssetMenu(menuName = "Units/AdventureMushroomAssetMap")]
public class AdventureMushroomAssetMap : ScriptableObject, IUnitViewDefinition<MushroomCollectible>, IUnitStatsProvider
{
    [Serializable]
    private struct Entry
    {
        public UnitDataSO UnitData;
        public MushroomCollectible Prefab;
    }
    public IEnumerable<UnitType> Types => _lookup?.Keys ?? Enumerable.Empty<UnitType>();

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<UnitType, Entry> _lookup;

    private void OnEnable() => Rebuild();

    private void Rebuild()
    {
        _lookup = entries
            .Where(e => e.Prefab != null)
            .GroupBy(e => e.UnitData.Type)
            .ToDictionary(g => g.Key, g => g.Last());
    }

    public bool TryGetAsset(UnitType type, out MushroomCollectible view)
    {
        view = null;
        if (_lookup != null && _lookup.TryGetValue(type, out var entry))
        {
            view = entry.Prefab;
            return view != null;
        }
        return false;
    }

    public bool TryGetBaseStats(UnitType type, out UnitStats stats)
    {
        stats = null;
        if (_lookup != null && _lookup.TryGetValue(type, out var entry))
        {
            stats = entry.UnitData.UnitStats;
            return true;
        }
        return false;
    }

    public bool TryGetDefinition(UnitType type, out UnitDataSO definition)
    {
        if (_lookup != null && _lookup.TryGetValue(type, out var entry))
        {
            definition = entry.UnitData;
            return true;
        }

        definition = default;
        return false;
    }


#if UNITY_INCLUDE_TESTS
    [Serializable]
    public struct TestEntry
    {
        public UnitType Type;
        public MushroomCollectible Prefab;
        public string DisplayName;
        public string Description;
        public UnitSharedDataSO SharedData;
        public UnitStatsSource Stats;
    }

    public void SetTestEntries(IEnumerable<TestEntry> testEntries)
    {
        entries = testEntries?.Select(e => new Entry
        {
            UnitData =new(),
            //Prefab = e.Prefab,
            //DisplayName = e.DisplayName,
            //Description = e.Description,
            //SharedData = e.SharedData,
            //Stats = e.Stats
        }).ToList() ?? new List<Entry>();
        Rebuild();
    }
#endif
}
