using System;
using System.Collections.Generic;

[Serializable]
public class UnitStatsSource
{
    public bool UseInline;
    public UnitStats Reference; // when UseInline == false, use UnitStats from SO-holding asset or direct instance
    public UnitStatsInline Inline;     // used when UseInline == true
    public bool TryGetUnitStats(out UnitStats stats)
    {
        stats = null;
        if (!UseInline)
        {
            stats = Reference;
            return stats != null;
        }

        stats = new UnitStats
        {
            Health = Inline.Health,
            MaxHealth = Inline.MaxHealth,
            Damage = Inline.Damage,
            SpellPower = Inline.SpellPower,
            Offense = Inline.Offense,
            Defense = Inline.Defense,
            MoveSpeed = Inline.MoveSpeed,
            AttackRange = Inline.AttackRange,
            CanFly = Inline.CanFly,
            InvulnerableEffects = Inline.InvulnerableEffects != null ? new List<StatusEffectType>(Inline.InvulnerableEffects) : new List<StatusEffectType>()
        };
        return true;
    }
}
