using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class UnitModelFactory
{
    List<UnitDefinitionSO> UnitDefinitionSOs;
    readonly Dictionary<UnitType, UnitDefinitionSO> _dataMap;
    public UnitModelFactory(
        IEnumerable<UnitDefinitionSO> allUnitDatas )
    {
        _dataMap = allUnitDatas.ToDictionary(d => d.UnitType);
    }
    public UnitModel Create(UnitSpawnParams unitSpawnParams)
    {
        UnitDefinitionSO unitDefinitionSO = _dataMap[unitSpawnParams.UnitType];
        return new UnitModel(unitDefinitionSO, unitSpawnParams.X,unitSpawnParams.Y, unitSpawnParams.Amount,unitSpawnParams.IsPlayer);
    }
}