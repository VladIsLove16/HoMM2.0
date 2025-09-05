using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Менеджер для управления префабами юнитов
/// </summary>
public class UnitPrefabManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private UnitPrefabManagerConfig _config;
    
    [Inject]
    private void Construct()
    {
        if (_config == null)
        {
            Debug.LogError("[UnitPrefabManager] Configuration is not assigned!");
            return;
        }
        
        _config.ValidateAllPrefabs();
    }
    
    /// <summary>
    /// Получает префаб для указанного типа юнита и режима игры
    /// </summary>
    public GameObject GetPrefab(UnitType unitType, GameMode gameMode)
    {
        if (_config == null)
        {
            Debug.LogError("[UnitPrefabManager] Configuration is not assigned!");
            return null;
        }
        
        return _config.GetPrefab(unitType, gameMode);
    }
    
    /// <summary>
    /// Проверяет, есть ли префаб для указанного типа юнита
    /// </summary>
    public bool HasPrefab(UnitType unitType)
    {
        if (_config == null)
        {
            Debug.LogError("[UnitPrefabManager] Configuration is not assigned!");
            return false;
        }
        
        return _config.HasPrefab(unitType);
    }
    
    /// <summary>
    /// Получает все настроенные типы юнитов
    /// </summary>
    public IEnumerable<UnitType> GetAvailableUnitTypes()
    {
        if (_config == null)
        {
            Debug.LogError("[UnitPrefabManager] Configuration is not assigned!");
            return new List<UnitType>();
        }
        
        return _config.GetAvailableUnitTypes();
    }
}
