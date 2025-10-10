using System.Collections.Generic;
using Adventure.Domain.Inventory;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Mushroom Catalog", fileName = "MushroomCatalog")]
public sealed class MushroomCatalogSO : ScriptableObject, IMushroomCatalog
{
    [System.Serializable]
    private struct Entry
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public Sprite IconHovered;
        public Sprite HumanizedIcon;
        public Sprite HumanizedIconHovered;
        public List<string> Characteristics;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();
    private readonly Dictionary<string, MushroomItem> _items = new Dictionary<string, MushroomItem>();

    private void OnEnable()
    {
        Cache();
    }

    private void Cache()
    {
        _items.Clear();
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.Id)) continue;
            _items[entry.Id] = new MushroomItem(entry.Id, entry.DisplayName, entry.Description);
        }
    }

    public bool TryGet(string id, out MushroomItem item)
    {
        if (_items.Count != entries.Count)
        {
            Cache();
        }
        return _items.TryGetValue(id, out item);
    }

    public IReadOnlyList<MushroomItem> GetAll()
    {
        if (_items.Count != entries.Count)
        {
            Cache();
        }
        return new List<MushroomItem>(_items.Values);
    }

    public bool TryGetVisuals(string id, out MushroomBookEntryViewData visuals)
    {
        foreach (var entry in entries)
        {
            if (entry.Id == id)
            {
                var characteristics = entry.Characteristics != null
                    ? new List<string>(entry.Characteristics)
                    : new List<string>();
                visuals = new MushroomBookEntryViewData(
                    entry.DisplayName,
                    entry.Description,
                    entry.Icon,
                    entry.IconHovered,
                    entry.HumanizedIcon,
                    entry.HumanizedIconHovered,
                    characteristics);
                return true;
            }
        }

        visuals = MushroomBookEntryViewData.Empty;
        return false;
    }
}
