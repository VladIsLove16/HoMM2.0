// UnitDefinitionSO.cs
using System.Collections.Generic;
using UnityEngine;
// UnitDefinitionSO.cs
[CreateAssetMenu(menuName = "Units/UnitDefinition data")]
public class UnitDefinitionSO : ScriptableObject
{
    public GameObject UnitViewPrefab;
    public Sprite UnitIcon;
    public string Name;
    public Material BlueTeamMaterial;
    public Material HoveredBlueTeamMaterial;
    public Material RedTeamMaterial;
    public Material HoveredRedTeamMaterial;
    public UnitType UnitType;
    public UnitStats Stats;
    [SerializeField] private List<StatusEffectData> startingEffects = new List<StatusEffectData>();
    [SerializeField] private List<StatusEffectData> invulnerableEffects = new List<StatusEffectData>();
    public IReadOnlyList<StatusEffectData> StartingEffects => startingEffects;
    public IReadOnlyList<StatusEffectData> InvulnerableEffects => invulnerableEffects;

}
