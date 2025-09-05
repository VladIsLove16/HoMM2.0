using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Менеджер для управления префабами юнитов
/// </summary>
public class UnitPrefabManager : MonoBehaviour
{
    [System.Serializable]
    public class UnitPrefabVariant
    {
        public UnitType UnitType;
        public GameObject LocalVariant;
        public GameObject NetworkVariant;
        
        public GameObject GetVariant(GameMode gameMode)
        {
            return gameMode switch
            {
                GameMode.Singleplayer => LocalVariant,
                GameMode.Multiplayer => NetworkVariant,
                _ => LocalVariant
            };
        }
        
        public bool IsValid()
        {
            return LocalVariant != null && NetworkVariant != null;
        }
    }
    
    [Header("Unit Prefab Variants")]
    [SerializeField] private List<UnitPrefabVariant> _prefabVariants = new();
    
    private Dictionary<UnitType, UnitPrefabVariant> _prefabMap;
    
    [Inject]
    private void Construct()
    {
        _prefabMap = new Dictionary<UnitType, UnitPrefabVariant>();
        foreach (var variant in _prefabVariants)
        {
            if (variant.IsValid())
            {
                _prefabMap[variant.UnitType] = variant;
            }
            else
            {
                Debug.LogWarning($"[UnitPrefabManager] Invalid prefab variant for unit type: {variant.UnitType}");
            }
        }
    }
    
    /// <summary>
    /// Получает префаб для указанного типа юнита и режима игры
    /// </summary>
    public GameObject GetPrefab(UnitType unitType, GameMode gameMode)
    {
        if (_prefabMap.TryGetValue(unitType, out var variant))
        {
            return variant.GetVariant(gameMode);
        }
        
        Debug.LogError($"[UnitPrefabManager] Prefab variant not found for unit type: {unitType}");
        return null;
    }
    
    /// <summary>
    /// Проверяет, есть ли префаб для указанного типа юнита
    /// </summary>
    public bool HasPrefab(UnitType unitType)
    {
        return _prefabMap.ContainsKey(unitType);
    }
    
    /// <summary>
    /// Получает все настроенные типы юнитов
    /// </summary>
    public IEnumerable<UnitType> GetAvailableUnitTypes()
    {
        return _prefabMap.Keys;
    }
    
    /// <summary>
    /// Валидирует все префабы
    /// </summary>
    [ContextMenu("Validate All Prefabs")]
    public void ValidateAllPrefabs()
    {
        foreach (var variant in _prefabVariants)
        {
            if (!variant.IsValid())
            {
                Debug.LogError($"[UnitPrefabManager] Invalid prefab variant: {variant.UnitType}");
            }
        }
    }
}
