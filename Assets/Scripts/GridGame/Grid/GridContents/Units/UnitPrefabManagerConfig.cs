using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Конфигурация для UnitPrefabManager
/// </summary>
[CreateAssetMenu(menuName = "Game/UnitPrefabManagerConfig")]
public class UnitPrefabManagerConfig : ScriptableObject
{
    [SerializeField] private UnitDefinitionSOCollection unitDefinitionSOCollection;
    /// <summary>
    /// Получает префаб для указанного типа юнита
    /// </summary>
    public UnitView3D GetPrefab(UnitType unitType)
    {
        var so = unitDefinitionSOCollection.GetAll().First(x => x.UnitType == unitType);
        return so.UnitView3DPrefab;
    }
    
    /// <summary>
    /// Проверяет, есть ли префаб для указанного типа юнита
    /// </summary>
    public bool HasPrefab(UnitType unitType)
    {
        return unitDefinitionSOCollection.GetAll().First(x => x.UnitType == unitType) != null;
    }
}

