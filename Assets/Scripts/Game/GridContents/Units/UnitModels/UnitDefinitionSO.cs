// UnitDefinitionSO.cs
using System;
using System.Collections.Generic;
using UnityEngine;

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
    public Sprite Icon;
    [Tooltip("Иконка при наведении (обычное представление)")]
    public Sprite HoveredIcon;
    [Tooltip("Иконка в хуманизированном представлении")]
    public Sprite HumanizedIcon;
    [Tooltip("Иконка в хуманизированном представлении при наведении")]
    public Sprite HumanizedHoveredIcon;
    public string Name;
    [SerializeField] private string displayName;

    [SerializeField] private MaterialProvider _materialProviderSerialized = new MaterialProvider();
    // Runtime provider can be any IMaterialProvider (including mocks in tests)
    private IMaterialProvider _materialProviderRuntime;

    /// <summary>
    /// model
    /// </summary>
    // NOTE: UnitStats is a ScriptableObject and must NOT be constructed with 'new'.
    // Initializing it here caused Unity to call ScriptableObject.ctor during
    // serialization which throws. Leave null by default and allow assets/tests
    // to assign a UnitStats instance (via inspector or ScriptableObject.CreateInstance).
    public UnitStats Stats;

    [Header("Unit Data for Book")]
    [TextArea(2, 6)]
    [SerializeField] private string description;
    [SerializeField] private List<StatusEffectData> startingEffects = new List<StatusEffectData>();
    [SerializeField] private List<StatusEffectData> invulnerableEffects = new List<StatusEffectData>();
    public IReadOnlyList<StatusEffectData> StartingEffects => startingEffects;
    public IReadOnlyList<StatusEffectData> InvulnerableEffects => invulnerableEffects;
    public string Description => description;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? Name : displayName;
    public Material GetBlueTeamMaterial() => materialProvider?.GetBlueTeamMaterial();
    public Material GetHoveredBlueTeamMaterial() => materialProvider?.GetHoveredBlueTeamMaterial();
    public Material GetRedTeamMaterial() => materialProvider?.GetRedTeamMaterial();
    public Material GetHoveredRedTeamMaterial() => materialProvider?.GetHoveredRedTeamMaterial();

    public void SetMaterialProvider(IMaterialProvider provider)
    {
        // Allow test mocks (any IMaterialProvider) to be used at runtime.
        _materialProviderRuntime = provider;
    }

    // Internal accessor that chooses runtime provider if set, otherwise serialized provider
    private IMaterialProvider materialProvider => _materialProviderRuntime ?? _materialProviderSerialized;

    /// <summary>
    /// Валидирует конфигурацию юнита
    /// </summary>
    /// <returns>True, если конфигурация корректна</returns>
    public bool IsValid()
    {
        return UnitViewPrefab != null && Stats != null && !string.IsNullOrEmpty(Name);
    }
}
