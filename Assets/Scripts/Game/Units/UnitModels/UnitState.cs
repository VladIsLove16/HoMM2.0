using System;
using System.Collections.Generic;
[Serializable]
public class UnitState 
{
    private UnitDefinitionSO unitDefinitionSO;
    public UnitState(UnitDefinitionSO unitDefinitionSO)
    {
        this.unitDefinitionSO = unitDefinitionSO;
        MaxHealth = unitDefinitionSO.BaseHealth;
        Health = unitDefinitionSO.BaseHealth;
        Damage = unitDefinitionSO.BaseDamage;
        Offense = unitDefinitionSO.BaseOffense;
        Defense = unitDefinitionSO.BaseDefense;
        MoveSpeed = unitDefinitionSO.BaseSpeed;
    }

    public int MaxHealth { get; set; }
    public int MoveSpeed { get; set; }
    public int Health { get; set; }
    public int Damage { get; set; }
    public int Offense { get; set; }
    public int Defense { get; set; }
    public int LastDamageAmount { get; set; }
}
