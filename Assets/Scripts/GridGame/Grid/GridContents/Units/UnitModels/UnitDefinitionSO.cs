// UnitDefinitionSO.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Units/UnitDefinition data")]
public class UnitDefinitionSO : ScriptableObject
{
    public UnitType UnitType;
    [Header("Unit Prefab")]
    public GameObject UnitViewPrefab;
    public UnitView3D UnitView3DPrefab;

    [Header("Unit Appearance")]
    [Tooltip("Иконка по умолчанию (обычное представление)")]
    public Sprite Icon;
    [Tooltip("Иконка при наведении (обычное представление)")]
    public Sprite HoveredIcon;
    [Tooltip("Иконка в хуманизированном представлении")]
    public Sprite HumanizedIcon;
    [Tooltip("Иконка в хуманизированном представлении при наведении")]
    public Sprite HumanizedHoveredIcon;
    [SerializeField] private string displayName;
    [SerializeField] private MaterialProvider _materialProviderSerialized = new MaterialProvider();
    [Header("Unit Data for Book")]
    [TextArea(2, 6)]
    [SerializeField] private string description;
    [SerializeField] private List<StatusEffectData> startingEffects = new List<StatusEffectData>();
    [SerializeField] private List<StatusEffectData> invulnerableEffects = new List<StatusEffectData>();
    public IReadOnlyList<StatusEffectData> StartingEffects => startingEffects;
    public IReadOnlyList<StatusEffectData> InvulnerableEffects => invulnerableEffects;
    public string Description => description;
    public string DisplayName
    {
        get
        {
           return string.IsNullOrWhiteSpace(displayName) ? UnitType.ToString() : displayName;

        }
        set
        {
            displayName = value;
        }
    }
    public Material GetBlueTeamMaterial() => materialProvider?.GetBlueTeamMaterial();
    public Material GetHoveredBlueTeamMaterial() => materialProvider?.GetHoveredBlueTeamMaterial();
    public Material GetRedTeamMaterial() => materialProvider?.GetRedTeamMaterial();
    public Material GetHoveredRedTeamMaterial() => materialProvider?.GetHoveredRedTeamMaterial();
    [SerializeField]  private  UnitStats Stats;
    public UnitStats UnitStats { get { return Stats; } set { Stats = value; } }
    public void SetMaterialProvider(IMaterialProvider provider)
    {
        // Allow test mocks (any IMaterialProvider) to be used at runtime.
        _materialProviderRuntime = provider;
    }
    private IMaterialProvider _materialProviderRuntime;
    private IMaterialProvider materialProvider => _materialProviderRuntime ?? _materialProviderSerialized;
}
