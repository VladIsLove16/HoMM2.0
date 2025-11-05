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
    }
    
    /// <summary>
    /// Получает префаб для указанного типа юнита и режима игры
    /// </summary>
    public UnitView3D GetPrefab(UnitType unitType)
    {
        if (_config == null)
        {
            Debug.LogError("[UnitPrefabManager] Configuration is not assigned!");
            return null;
        }
        
        return _config.GetPrefab(unitType );
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
}
