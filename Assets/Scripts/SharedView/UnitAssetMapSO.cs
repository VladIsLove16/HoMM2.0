using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Units/Unit Asset Map")]
public class UnitAssetMapSO<TAsset> : ScriptableObject, IUnitViewDefinition<TAsset>
    where TAsset : UnityEngine.Object
{
    [Serializable]
    private struct Entry
    {
        public UnitType Type;
        public TAsset Asset;
    }

    [SerializeField] private List<Entry> entries = new();
    private Dictionary<UnitType, TAsset> _lookup;

    private void OnEnable() => Rebuild();

    private void Rebuild()
    {
        _lookup = entries
            .Where(e => e.Asset != null)
            .GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.Last().Asset);
    }

    public bool TryGetAsset(UnitType type, out TAsset asset)
    {
        asset = null;
        return _lookup != null && _lookup.TryGetValue(type, out asset) && asset != null;
    }
}
