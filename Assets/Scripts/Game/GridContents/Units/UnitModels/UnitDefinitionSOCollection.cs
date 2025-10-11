// UnitDefinitionSOCollection.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "UnitDefinitionSOCollection")]
public class UnitDefinitionSOCollection : ScriptableObject
{
    [SerializeField] List<UnitDefinitionSO> unitDefinitionSOs;
    private Dictionary<UnitType, UnitDefinitionSO> _definitionsByUnitType = new Dictionary<UnitType, UnitDefinitionSO>();

    public IReadOnlyDictionary<UnitType, UnitDefinitionSO> ToDictionary()
    {
        return unitDefinitionSOs.ToDictionary(x => x.UnitType);
    }
    private void OnEnable()
    {
        Cache();
    }

    private void Cache()
    {
        _definitionsByUnitType.Clear();

        if (_definitionsByUnitType == null)
            return;

        foreach (var definition in unitDefinitionSOs)
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
        if (_definitionsByUnitType.Count == (unitDefinitionSOs?.Count ?? 0))
            return;

        Cache();
    }
    public bool TryGet(UnitType id, out UnitDefinitionSO item)
    {
        EnsureCache();
        return _definitionsByUnitType.TryGetValue(id, out item);
    }

    public IReadOnlyList<UnitDefinitionSO> GetAll()
    {
        EnsureCache();
        return unitDefinitionSOs;
    }
    //public bool TryGetVisuals(UnitType id, out MushroomViewModel visuals)
    //{
    //    EnsureCache();
    //    if (_items.TryGetValue(id, out var item) && _definitionsByUnitType.TryGetValue(id, out var definition))
    //    {
    //        var characteristics = BuildCharacteristics(item);
    //        visuals = new UnitViewModel(
    //            id,
    //            item.DisplayName,
    //            item.Description,
    //            definition.UnitIcon,
    //            definition.UnitIconHovered,
    //            definition.HumanizedIcon,
    //            definition.HumanizedIconHovered,
    //            characteristics);
    //        return true;
    //    }

    //    visuals = MushroomViewModel.Empty;
    //    return false;
    //}
    private static IReadOnlyList<string> BuildCharacteristics(UnitModel item)
    {
        var stats = item.ModifiedStats;
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
