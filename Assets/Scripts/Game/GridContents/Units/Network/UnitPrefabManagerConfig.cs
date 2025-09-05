using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Конфигурация для UnitPrefabManager
/// </summary>
[CreateAssetMenu(menuName = "Game/UnitPrefabManagerConfig")]
public class UnitPrefabManagerConfig : ScriptableObject
{
    [System.Serializable]
    public class UnitPrefabVariant
    {
        [Header("Unit Configuration")]
        public UnitType UnitType;
        
        [Header("Prefab Variants")]
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
    
    public List<UnitPrefabVariant> PrefabVariants => _prefabVariants;
    
    /// <summary>
    /// Получает префаб для указанного типа юнита и режима игры
    /// </summary>
    public GameObject GetPrefab(UnitType unitType, GameMode gameMode)
    {
        foreach (var variant in _prefabVariants)
        {
            if (variant.UnitType == unitType)
            {
                return variant.GetVariant(gameMode);
            }
        }
        
        Debug.LogError($"[UnitPrefabManagerConfig] Prefab variant not found for unit type: {unitType}");
        return null;
    }
    
    /// <summary>
    /// Проверяет, есть ли префаб для указанного типа юнита
    /// </summary>
    public bool HasPrefab(UnitType unitType)
    {
        foreach (var variant in _prefabVariants)
        {
            if (variant.UnitType == unitType)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Получает все настроенные типы юнитов
    /// </summary>
    public IEnumerable<UnitType> GetAvailableUnitTypes()
    {
        foreach (var variant in _prefabVariants)
        {
            yield return variant.UnitType;
        }
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
                Debug.LogError($"[UnitPrefabManagerConfig] Invalid prefab variant: {variant.UnitType}");
            }
        }
    }
}
