using System;
using System.Collections.Generic;
using System.Linq;
using SharedView.Audio;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Units/Grid Unit Map")]
public class GridUnitAssetMap : ScriptableObject, IUnitViewDefinition<UnitView3D>, IUnitStatsProvider, IUnitAudioProfileProvider
{
    [Serializable]
    private struct Entry
    {
        public UnitDataSO UnitDataSO;
        public UnitView3D Prefab;
        public bool UseAddressables;
        public AssetReferenceGameObject PrefabReference;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<UnitType, Entry> _lookup;

    public IEnumerable<UnitType> Types => _lookup?.Keys ?? Enumerable.Empty<UnitType>();

    private void OnEnable() => Rebuild();

    private void Rebuild()
    {
        _lookup = entries
            .Where(e => e.UnitDataSO != null)
            .GroupBy(e => e.UnitDataSO.Type)
            .ToDictionary(g => g.Key, g => g.Last());
    }

    public bool TryGetAsset(UnitType type, out UnitView3D view)
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
            stats = entry.UnitDataSO.UnitStats;
            return true;
        }
        return false;
    }

    public bool TryGetShared(UnitType type, out UnitSharedDataSO shared)
    {
        shared = null;
        return _lookup != null && _lookup.TryGetValue(type, out var entry) && (shared = entry.UnitDataSO.SharedData) != null;
    }

    public bool TryGetAudioProfile(UnitType type, out UnitAudioProfile profile)
    {
        profile = null;
        return _lookup != null && _lookup.TryGetValue(type, out var entry) && (profile = entry.UnitDataSO.AudioProfile) != null;
    }

    public bool TryGetAssetReference(UnitType type, out AssetReferenceGameObject prefabReference)
    {
        prefabReference = null;

        if (_lookup == null || !_lookup.TryGetValue(type, out var entry))
            return false;

        if (!entry.UseAddressables || entry.PrefabReference == null)
            return false;

        return entry.PrefabReference.RuntimeKeyIsValid() && (prefabReference = entry.PrefabReference) != null;
    }

#if UNITY_INCLUDE_TESTS
    [Serializable]
    public struct TestEntry
    {
        public UnitType Type;
        public UnitView3D Prefab;
        public UnitStatsSource Stats;
        public UnitSharedDataSO SharedData;
    }

    public void SetTestEntries(IEnumerable<TestEntry> testEntries)
    {
        entries = testEntries?.Select(e => new Entry
        {
            Prefab = e.Prefab,
            //Type = e.Type,
            //Stats = e.Stats,
            //SharedData = e.SharedData
        }).ToList() ?? new List<Entry>();
        Rebuild();
    }
#endif
}
