using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public class UnitModelFactory
{
    [Inject] readonly IReadOnlyDictionary<UnitType, UnitDefinitionSO> _dataMap;
    public UnitModelFactory( )
    {
        
    }
    public UnitModel Create(UnitSpawnParams unitSpawnParams)
    {
        UnitDefinitionSO unitDefinitionSO = _dataMap[unitSpawnParams.UnitType];
        return new UnitModel(unitDefinitionSO.Stats, unitSpawnParams.UnitType, unitSpawnParams.X,unitSpawnParams.Y, unitSpawnParams.Amount,unitSpawnParams.IsPlayer);
    }
}