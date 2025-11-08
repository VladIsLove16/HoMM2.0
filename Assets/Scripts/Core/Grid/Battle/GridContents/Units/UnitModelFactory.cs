using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

public class UnitModelFactory
{
    [Inject] IUnitStatsProvider _unitData;
    public UnitModelFactory(IUnitStatsProvider unitData)
    {
        _unitData = unitData;
        UnityLogger.Log("UnitModelFactory init with UnitDatabase");
    }
    public UnitModelFactory(IReadOnlyDictionary<UnitType, UnitStats> definitions)
    {
        if (definitions == null)
            throw new ArgumentNullException(nameof(definitions));
        _unitData = new DictionaryUnitStatsProvider(definitions);
    }
    public UnitModelFactory(bool loadFromResource = true)
    {
        if (loadFromResource)
        {
            try
            {
                //_dataMap = DataMapFromResources();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"UnitModelFactory: failed to load unit data from resources: {ex.Message}");
            }
        }
    }
    public UnitModel Create(UnitSpawnParams unitSpawnParams)
    {
        if(_unitData == null)
        {
            throw new InvalidOperationException("Unit data provider is not initialized");
        }

        // Resolve base stats via unified provider
        if (!_unitData.TryGetBaseStats(unitSpawnParams.UnitType, out var baseStats) || baseStats == null)
        {
            Debug.LogError($"No stats source for UnitType {unitSpawnParams.UnitType}. Creating for first key in {String.Join(" ", _unitData.Types)}");
        }

        return new UnitModel(baseStats, unitSpawnParams.UnitType, unitSpawnParams.X, unitSpawnParams.Y, unitSpawnParams.Amount, unitSpawnParams.Team);
    }
    //private IReadOnlyDictionary<UnitType, UnitDefinitionSO> DataMapFromResources()
    //{
    //    if (!Application.isPlaying)
    //    {
    //        Debug.Log("UnitModelFactory: Skipping UnitDefinitionSOCollection load because Application.isPlaying == false");
    //        return new Dictionary<UnitType, UnitDefinitionSO>();
    //    }

    //    //var gameUnitDatas = AssetDatabase.LoadAssetAtPath<UnitDefinitionSOCollection>("Assets/ScriptableObjects/Game/UnitDefinitionSOCollection.asset");
    //    //if (gameUnitDatas == null)
    //    //{
    //    //    Debug.LogWarning("UnitModelFactory: UnitDefinitionSOCollection asset not found at Assets/ScriptableObjects/Game/UnitDefinitionSOCollection.asset");
    //    //    return new Dictionary<UnitType, UnitDefinitionSO>();
    //    //}

    //    //var dict = gameUnitDatas.ToDictionary();
    //    //if (dict == null)
    //    //{
    //    //    return new Dictionary<UnitType, UnitDefinitionSO>();
    //    //}

    //    return dict;
    //}   
}

internal sealed class DictionaryUnitStatsProvider : IUnitStatsProvider
{
    private readonly IReadOnlyDictionary<UnitType, UnitStats> _definitions;

    public DictionaryUnitStatsProvider(IReadOnlyDictionary<UnitType, UnitStats> definitions)
    {
        _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    }

    public IEnumerable<UnitType> Types => _definitions.Keys;

    public bool TryGetBaseStats(UnitType type, out UnitStats stats)
    {
        return _definitions.TryGetValue(type, out stats) && stats != null;
    }
}
