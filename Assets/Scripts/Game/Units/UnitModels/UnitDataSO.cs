// UnitDataSO.cs
using System.Collections.Generic;
using UnityEngine;
// UnitDataSO.cs
[CreateAssetMenu(menuName = "Units/UnitViewModel Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Основные характеристики")]
    public string UnitName;
    public UnitType UnitType;
    public int Health;
    public int Damage;

    [Header("Стартовые статус-эффекты")]
    public List<StatusEffectData> StartingEffects;

    public GameObject UnitViewPrefab;
}
