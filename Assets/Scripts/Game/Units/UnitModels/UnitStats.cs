using System;
using System.Collections.Generic;
[Serializable]
public class UnitStats 
{
    private UnitDefinitionSO unitDefinitionSO;

    public UnitStats(UnitDefinitionSO unitDefinitionSO)
    {
        MaxHealth = unitDefinitionSO.BaseHealth;
        Health = unitDefinitionSO.BaseHealth;
        Damage = unitDefinitionSO.BaseDamage;
        Offense = unitDefinitionSO.BaseOffense;
        Defense = unitDefinitionSO.BaseDefense;
        Speed = unitDefinitionSO.BaseSpeed;
    }

    public int MaxHealth { get; set; }
    public int Speed { get; set; }
    public int Health { get; set; }
    public int Damage { get; set; }
    public int Offense { get; set; }
    public int Defense { get; set; }

    public int LastDamageAmount { get; internal set; }
    public StatusEffectManager StatusEffectManager { get; internal set; }
}
