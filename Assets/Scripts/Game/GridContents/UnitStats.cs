using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UniRx;
using UnityEngine;
[CreateAssetMenu(menuName = "Units/new BaseUnitStats")]
public class UnitStats : ScriptableObject
{
    public int Health =0;
    public int MaxHealth = 0;
    public int Damage = 0;
    public int SpellPower = 0;

    public int Offense = 0;
    public int Defense = 0;
    public int MoveSpeed = 0;
    public int AttackRange = 0;
    public bool CanFly = false;
    public bool? IsSkeleton;
    [SerializeField] private List<StatusEffectType> invulnerableEffects;
    public List<StatusEffectType> InvulnerableEffects
    {
        get {
            if (invulnerableEffects == null)
            {
                invulnerableEffects = new();
            }
            return invulnerableEffects; 
        }
        set
            { invulnerableEffects = value; }
    }
    public static UnitStats operator +(UnitStats baseStats, UnitStats modifier)
    {
        if(baseStats == null )
            throw new ArgumentException();
        if (modifier == null) 
            throw new ArgumentException();
        var invulnerableEffects = baseStats.InvulnerableEffects.ToList();
        invulnerableEffects.AddRange(modifier.InvulnerableEffects);
        baseStats.Health += modifier.Health;
        baseStats.MaxHealth += modifier.MaxHealth;
        baseStats.Damage += modifier.Damage;
        baseStats.SpellPower += modifier.SpellPower;
        baseStats.Offense += modifier.Offense;
        baseStats.Defense += modifier.Defense;
        baseStats.MoveSpeed += modifier.MoveSpeed;
        baseStats.AttackRange += modifier.AttackRange;
        return baseStats;
    }
}