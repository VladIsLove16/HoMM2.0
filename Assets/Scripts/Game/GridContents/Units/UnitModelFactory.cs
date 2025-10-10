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
    [Inject] IReadOnlyDictionary<UnitType, UnitDefinitionSO> _dataMap;
    public UnitModelFactory(IReadOnlyDictionary<UnitType, UnitDefinitionSO> dataMap)
    {
        _dataMap = dataMap;
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
                _dataMap = new Dictionary<UnitType, UnitDefinitionSO>();
            }
        }
    }
    public UnitModel Create(UnitSpawnParams unitSpawnParams)
    {
        if(_dataMap == null)
        {
            throw new InvalidOperationException("Data map is not initialized");
        }
        if (!_dataMap.TryGetValue(unitSpawnParams.UnitType, out var unitDefinitionSO) || unitDefinitionSO == null)
        {
            throw new InvalidOperationException($"UnitDefinitionSO not found for UnitType {unitSpawnParams.UnitType}");
        }
        return new UnitModel(unitDefinitionSO.Stats, unitSpawnParams.UnitType, unitSpawnParams.X, unitSpawnParams.Y, unitSpawnParams.Amount, unitSpawnParams.Team);
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