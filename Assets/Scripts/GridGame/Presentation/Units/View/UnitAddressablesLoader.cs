#if ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Utility for loading and unloading unit prefabs via Addressables based on GridUnitAssetMap configuration.
/// If an entry in the map has <c>UseAddressables</c> enabled and a valid <c>AssetReference</c>,
/// the prefab will be resolved through Addressables; otherwise the direct prefab reference is used.
/// </summary>
public sealed class UnitAddressablesLoader
{
    private readonly GridUnitAssetMap _assetMap;
    private readonly Dictionary<UnitType, AsyncOperationHandle<GameObject>> _handles = new();
    private readonly Dictionary<UnitType, GameObject> _loadedPrefabs = new();

    public UnitAddressablesLoader(GridUnitAssetMap assetMap)
    {
        _assetMap = assetMap ? assetMap : throw new ArgumentNullException(nameof(assetMap));
    }

    /// <summary>
    /// Loads a unit prefab for the given type, either from Addressables (when configured)
    /// or from the direct prefab reference stored in <see cref="GridUnitAssetMap"/>.
    /// </summary>
    public async Task<GameObject> LoadUnitPrefabAsync(UnitType type)
    {
        if (_loadedPrefabs.TryGetValue(type, out var cached))
        {
            return cached;
        }

        // Prefer direct prefab if available
        if (_assetMap.TryGetAsset(type, out var directPrefab) && directPrefab != null)
        {
            cached = directPrefab.gameObject;
            _loadedPrefabs[type] = cached;
            return cached;
        }

        // Fallback to Addressables when explicitly configured
        if (_assetMap.TryGetAssetReference(type, out var reference))
        {
            var handle = reference.LoadAssetAsync();
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Addressables.Release(handle);
                throw new InvalidOperationException(
                    $"Failed to load Addressable unit prefab for type '{type}' (reference key: {reference.RuntimeKey}).");
            }

            cached = handle.Result;
            _handles[type] = handle;
            _loadedPrefabs[type] = cached;
            return cached;
        }

        throw new InvalidOperationException(
            $"No prefab or Addressable reference configured for unit type '{type}' in GridUnitAssetMap.");
    }

    /// <summary>
    /// Preloads all unit prefabs required for a single battle based on the provided unit types.
    /// </summary>
    public async Task PreloadForBattleAsync(IEnumerable<UnitType> unitTypes)
    {
        if (unitTypes == null)
            return;

        foreach (var type in unitTypes)
        {
            _ = await LoadUnitPrefabAsync(type);
        }
    }

    /// <summary>
    /// Releases all Addressable-backed prefabs loaded through this loader.
    /// Direct prefab references are left untouched.
    /// Call this after the battle is finished to free memory.
    /// </summary>
    public void UnloadAllForBattle()
    {
        foreach (var handle in _handles.Values)
        {
            Addressables.Release(handle);
        }

        _handles.Clear();
        _loadedPrefabs.Clear();
    }
}
#endif
