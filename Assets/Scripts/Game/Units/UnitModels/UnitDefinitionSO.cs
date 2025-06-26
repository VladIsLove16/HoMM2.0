// UnitDefinitionSO.cs
using System.Collections.Generic;
using UnityEngine;
// UnitDefinitionSO.cs
[CreateAssetMenu(menuName = "Units/UnitDefinition Data")]
public class UnitDefinitionSO : ScriptableObject
{
    public GameObject UnitViewPrefab;
    public Sprite UnitIcon;
    public string Name;
    public UnitType UnitType;
    public int BaseHealth;
    public int BaseDamage;
    public int BaseOffense;
    public int BaseDefense;
    public int BaseSpeed;
    [SerializeField] private List<StatusEffectData> startingEffects = new List<StatusEffectData>();
    public IReadOnlyList<StatusEffectData> StartingEffects => startingEffects;

}
