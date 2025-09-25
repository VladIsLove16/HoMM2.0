using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Zenject;

public class UnitModelFactory
{
   Dictionary<UnitType, UnitDefinitionSO> _dataMap;
    public UnitModelFactory(Dictionary<UnitType, UnitDefinitionSO> dataMap)
    {
        _dataMap = dataMap;
    }
    public UnitModelFactory()
    {
        _dataMap = DataMapFromResources();
    }
    public UnitModel Create(UnitSpawnParams unitSpawnParams)
    {
        if(_dataMap == null)
        {
            throw new InvalidOperationException("Data map is not initialized");
        }
        UnitDefinitionSO unitDefinitionSO = _dataMap[unitSpawnParams.UnitType];
        return new UnitModel(unitDefinitionSO.Stats, unitSpawnParams.UnitType, unitSpawnParams.X,unitSpawnParams.Y, unitSpawnParams.Amount,unitSpawnParams.IsPlayer);
    }
    public void Add (UnitType unitType, UnitDefinitionSO unitDefinitionSO)
    {
        _dataMap[unitType] = unitDefinitionSO;
    }
    private Dictionary<UnitType, UnitDefinitionSO> DataMapFromResources()
    {
        var dict = new Dictionary<UnitType, UnitDefinitionSO>
            {
                { UnitType.Witch, Resources.Load<UnitDefinitionSO>("ScriptableObjects/Units/Witch/Witch") }
            };
        return dict;
    }   
}