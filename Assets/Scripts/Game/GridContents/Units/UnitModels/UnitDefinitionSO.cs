// UnitDefinitionSO.cs
using System.Collections.Generic;
using UnityEngine;

// Интерфейс для тестирования материалов
public interface IMaterialProvider
{
    Material GetBlueTeamMaterial();
    Material GetHoveredBlueTeamMaterial();
    Material GetRedTeamMaterial();
    Material GetHoveredRedTeamMaterial();
}

// Реальная реализация провайдера материалов
[System.Serializable]
public class MaterialProvider : IMaterialProvider
{
    [SerializeField] private Material _blueTeamMaterial;
    [SerializeField] private Material _hoveredBlueTeamMaterial;
    [SerializeField] private Material _redTeamMaterial;
    [SerializeField] private Material _hoveredRedTeamMaterial;

    public Material GetBlueTeamMaterial() => _blueTeamMaterial;
    public Material GetHoveredBlueTeamMaterial() => _hoveredBlueTeamMaterial;
    public Material GetRedTeamMaterial() => _redTeamMaterial;
    public Material GetHoveredRedTeamMaterial() => _hoveredRedTeamMaterial;

    public Material BlueTeamMaterial => _blueTeamMaterial;
    public Material HoveredBlueTeamMaterial => _hoveredBlueTeamMaterial;
    public Material RedTeamMaterial => _redTeamMaterial;
    public Material HoveredRedTeamMaterial => _hoveredRedTeamMaterial;
}

[CreateAssetMenu(menuName = "Units/UnitDefinition data")]
public class UnitDefinitionSO : ScriptableObject
{
    public UnitType UnitType;
    
    /// <summary>
    /// Основной префаб юнита (для обратной совместимости)
    /// </summary>
    [Header("Unit Prefab")]
    public GameObject UnitViewPrefab;
    
    [Header("Unit Appearance")]
    [Tooltip("Иконка по умолчанию (обычное представление)")]
    public Sprite UnitIcon;
    [Tooltip("Иконка при наведении (обычное представление)")]
    public Sprite UnitIconHovered;
    [Tooltip("Иконка в хуманизированном представлении")]
    public Sprite HumanizedIcon;
    [Tooltip("Иконка в хуманизированном представлении при наведении")]
    public Sprite HumanizedIconHovered;
    public string Name;
    
    [SerializeField] private MaterialProvider _materialProvider = new MaterialProvider();
    
    /// <summary>
    /// model
    /// </summary>
    public UnitStats Stats;
    
    [Header("Unit Data for Book")]
    [TextArea(2, 6)]
    [SerializeField] private string description;
    
    [System.Serializable]
    public class Characteristic
    {
        public string Key;
        public string Value;
    }
    [SerializeField] private List<Characteristic> characteristics = new List<Characteristic>();
    [SerializeField] private List<StatusEffectData> startingEffects = new List<StatusEffectData>();
    [SerializeField] private List<StatusEffectData> invulnerableEffects = new List<StatusEffectData>();
    
    public IReadOnlyList<StatusEffectData> StartingEffects => startingEffects;
    public IReadOnlyList<StatusEffectData> InvulnerableEffects => invulnerableEffects;
    public string Description => description;
    public IReadOnlyList<Characteristic> Characteristics => characteristics;
    
    // Методы для получения материалов (для тестирования)
    public Material GetBlueTeamMaterial() => _materialProvider.GetBlueTeamMaterial();
    public Material GetHoveredBlueTeamMaterial() => _materialProvider.GetHoveredBlueTeamMaterial();
    public Material GetRedTeamMaterial() => _materialProvider.GetRedTeamMaterial();
    public Material GetHoveredRedTeamMaterial() => _materialProvider.GetHoveredRedTeamMaterial();
    
    // Метод для установки провайдера материалов (для тестирования)
    public void SetMaterialProvider(IMaterialProvider provider)
    {
        if (provider is MaterialProvider materialProvider)
        {
            _materialProvider = materialProvider;
        }
    }
    
    /// <summary>
    /// Валидирует конфигурацию юнита
    /// </summary>
    /// <returns>True, если конфигурация корректна</returns>
    public bool IsValid()
    {
        return UnitViewPrefab != null && Stats != null && !string.IsNullOrEmpty(Name);
    }
}
