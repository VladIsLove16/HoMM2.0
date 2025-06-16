using System.Collections.Generic;
public class UnitStats 
{
    public int Health;
    public int MaxHealth;
    public int Damage;
    public int Amount;
    public UnitStats (UnitDataSO data, int amount)
    {
        MaxHealth = data.Health;
        Health = data.Health;
        Damage = data.Damage;
        Amount = amount;
    }

    public int LastDamageAmount { get; internal set; }
}
