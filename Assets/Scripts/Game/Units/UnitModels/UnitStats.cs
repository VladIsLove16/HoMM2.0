using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UniRx;
using UnityEngine;
[CreateAssetMenu(menuName = "Units/new BaseUnitStats")]
public class UnitStats : ScriptableObject
{
    public UnitType UnitType;
    public int Health;
    public int MaxHealth;
    public int Damage;
    public int SpellPower;
    public int Offense;
    public int Defense;
    public int MoveSpeed;
    public int AttackRange;
    public bool CanFly;
    public List<StatusEffectType> InvulnerableEffects;
    public static UnitStats operator +(UnitStats baseStats, UnitStats modifier)
    {
        var invulnerableEffects = new List<StatusEffectType>(baseStats.InvulnerableEffects);
        invulnerableEffects.AddRange(modifier.InvulnerableEffects);
        return new UnitStats
        {
            Health = baseStats.Health + modifier.Health,
            MaxHealth = baseStats.MaxHealth + modifier.MaxHealth,
            Damage = baseStats.Damage + modifier.Damage,
            SpellPower = baseStats.SpellPower + modifier.SpellPower,
            Offense = baseStats.Offense + modifier.Offense,
            Defense = baseStats.Defense + modifier.Defense,
            MoveSpeed = baseStats.MoveSpeed + modifier.MoveSpeed,
            AttackRange = baseStats.AttackRange + modifier.AttackRange,
            CanFly = modifier.CanFly,
            InvulnerableEffects = invulnerableEffects
        };
    }
}