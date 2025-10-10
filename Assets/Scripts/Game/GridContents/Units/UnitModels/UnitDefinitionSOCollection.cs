// UnitDefinitionSOCollection.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "UnitDefinitionSOCollection")]
public class UnitDefinitionSOCollection : ScriptableObject
{
    [SerializeField] List<UnitDefinitionSO> unitDefinitionSOs;

    public IReadOnlyDictionary<UnitType, UnitDefinitionSO> ToDictionary()
    {
        return unitDefinitionSOs.ToDictionary(x => x.UnitType);
    }
}
