using System.Collections.Generic;
using Adventure.Domain.Inventory;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Mushroom Catalog", fileName = "MushroomCatalog")]
public sealed class MushroomCatalogSO : ScriptableObject, IMushroomCatalog
{
    [SerializeField] private List<UnitDefinitionSO> mushroomDefinitions = new List<UnitDefinitionSO>();

    private readonly Dictionary<UnitType, UnitDefinitionSO> _definitionsByUnitType = new Dictionary<UnitType, UnitDefinitionSO>();

    private void OnEnable()
    {
        Cache();
    }

    private void Cache()
    {
        _definitionsByUnitType.Clear();

        if (mushroomDefinitions == null)
            return;

        foreach (var definition in mushroomDefinitions)
        {
            if (definition == null)
                continue;

            var UnitType = definition.UnitType;
            if (UnitType == UnitType.Archer)
                continue;

            _definitionsByUnitType[UnitType] = definition;
        }
    }

    private void EnsureCache()
    {
        if (_items.Count == (mushroomDefinitions?.Count ?? 0))
            return;

        Cache();
    }

    public bool TryGet(UnitType id, out MushroomModel item)
    {
        EnsureCache();
        return _items.TryGetValue(id, out item);
    }

    public IReadOnlyList<MushroomModel> GetAll()
    {
        EnsureCache();
        return new List<MushroomModel>(_items.Values);
    }

    public bool TryGetVisuals(UnitType id, out MushroomViewModel visuals)
    {
        EnsureCache();
        if (_items.TryGetValue(id, out var item) && _definitionsByUnitType.TryGetValue(id, out var definition))
        {
            var characteristics = BuildCharacteristics(item);
            visuals = new MushroomViewModel(
                id,
                item.DisplayName,
                item.Description,
                definition.UnitIcon,
                definition.UnitIconHovered,
                definition.HumanizedIcon,
                definition.HumanizedIconHovered,
                characteristics);
            return true;
        }

        visuals = MushroomViewModel.Empty;
        return false;
    }

    private static IReadOnlyList<string> BuildCharacteristics(MushroomModel item)
    {
        var stats = item.Stats;
        var result = new List<string>
        {
            $"Health: {stats.Health}/{stats.MaxHealth}",
            $"Damage: {stats.Damage}",
            $"Spell Power: {stats.SpellPower}",
            $"Offense: {stats.Offense}",
            $"Defense: {stats.Defense}",
            $"Move Speed: {stats.MoveSpeed}",
            $"Attack Range: {stats.AttackRange}",
            $"Can Fly: {(stats.CanFly ? "Yes" : "No")}"
        };

        if (stats.IsSkeleton.HasValue)
        {
            result.Add($"Skeleton: {(stats.IsSkeleton.Value ? "Yes" : "No")}");
        }

        if (stats.InvulnerableEffects != null && stats.InvulnerableEffects.Count > 0)
        {
            result.Add($"Immune: {string.Join(", ", stats.InvulnerableEffects)}");
        }

        return result;
    }
}
