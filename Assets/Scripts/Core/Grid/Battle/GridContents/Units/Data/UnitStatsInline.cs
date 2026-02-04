using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct UnitStatsInline
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [TextArea(2, 6)]
    [SerializeField] private string description;
    public string DisplayName => displayName;
    public string Description => description;
    [Header("Base Stats (inline)")]
    public int Health;
    public int MaxHealth;
    public int Damage;
    public int SpellPower;
    public int Offense;
    public int Defense;
    public int MoveSpeed;
    public int AttackRange;
    /// <summary>
    /// null = использовать значение по умолчанию (true).
    /// </summary>
    public bool? AllowAdjacentRanged;
    public bool CanFly;

    public bool? IsSkeleton;
    public List<StatusEffectType> StartingEffects;
    public List<StatusEffectType> InvulnerableEffects;
}
