using Adventure.Infrastructure.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TestDataFactory
{
    public static AdventureMushroomAssetMap CreateAdventureMushroomMap(IList<UnityEngine.Object> tracker, params (UnitType type, string displayName, UnitStatsInline stats)[] items)
    {
        var map = ScriptableObject.CreateInstance<AdventureMushroomAssetMap>();
        tracker?.Add(map);

        var entries = new List<AdventureMushroomAssetMap.TestEntry>();

        foreach (var item in items)
        {
            var prefabGo = new GameObject($"Mushroom_{item.type}");
            prefabGo.hideFlags = HideFlags.HideAndDontSave;
            var prefab = prefabGo.AddComponent<MushroomCollectible>();
            prefab.SetUnitType(item.type);
            tracker?.Add(prefabGo);

            var shared = ScriptableObject.CreateInstance<UnitSharedDataSO>();
            tracker?.Add(shared);

            entries.Add(new AdventureMushroomAssetMap.TestEntry
            {
                Type = item.type,
                Prefab = prefab,
                DisplayName = item.displayName ?? item.type.ToString(),
                Description = string.Empty,
                SharedData = shared,
                Stats = new UnitStatsSource
                {
                    UseInline = true,
                    Inline = item.stats
                }
            });
        }

        map.SetTestEntries(entries);
        return map;
    }

    public static AdventureMushroomAssetMap CreateAdventureMushroomMap(params (UnitType type, string displayName, UnitStatsInline stats)[] items)
        => CreateAdventureMushroomMap(null, items);

    public static AdventureMushroomAssetMap CreateAdventureMushroomMap(IList<UnityEngine.Object> tracker, params (UnitType type, UnitStatsInline stats)[] items)
    {
        return CreateAdventureMushroomMap(tracker, items.Select(i => (i.type, i.type.ToString(), i.stats)).ToArray());
    }

    public static AdventureMushroomAssetMap CreateSingleAdventureMushroom(UnitType type, int moveSpeed = 3, int health = 100)
        => CreateSingleAdventureMushroom(null, type, moveSpeed, health);

    public static AdventureMushroomAssetMap CreateSingleAdventureMushroom(IList<UnityEngine.Object> tracker, UnitType type, int moveSpeed = 3, int health = 100)
    {
        return CreateAdventureMushroomMap(
            tracker,
            (type, type.ToString(), new UnitStatsInline
            {
                MoveSpeed = moveSpeed,
                Health = health,
                MaxHealth = health,
                InvulnerableEffects = new List<StatusEffectType>()
            }));
    }
}
