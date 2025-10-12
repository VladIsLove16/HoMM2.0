// GameUnitDatas.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "GameUnitDatas")]
public class GameUnitDatas : ScriptableObject
{
   [SerializeField] List< UnitDefinitionSO> unitDefinitionSOs;

    public IReadOnlyDictionary<UnitType, UnitDefinitionSO> ToDictionary()
    {
        return unitDefinitionSOs.ToDictionary(x => x.UnitType);
    }
}