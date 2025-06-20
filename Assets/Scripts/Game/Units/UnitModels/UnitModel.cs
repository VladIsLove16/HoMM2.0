using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
//IBlockable, IEffectable, IAttackable,
public class UnitModel : IGridContent,  IDamageSource, IDamageable, IEffectable, IEffectApplier
{
    public event Action<DamageContext> OnBeforeDealDamage;
    public event Action<DamageContext> OnBeforeTakeDamage;
    public event Action<int> OnHealthChanged;
    public event Action OnTurnStart;
    public event Action OnDeath;
    public event Action OnAttack;
    public event Action OnHit;

    public UnitStats UnitStats { get; }
    public int X { get;}
    public int Y { get; }
    public int Amount { get; internal set; }
    public UnitType UnitType { get; }
    public string Name { get; }
    public bool IsPlayer { get; }
    public StatusEffectManager StatusEffectManager => UnitStats.StatusEffectManager;

    public UnitModel(UnitDefinitionSO unitDefinitionSO, int x,int y, int amount, bool isPlayer)
    {
        
        UnitStats = new UnitStats(unitDefinitionSO);
        foreach (var effect in unitDefinitionSO.StartingEffects)
        {
            StatusEffect statusEffect = new StatusEffect(effect, this, this);
            StatusEffectManager.Add(statusEffect);
        }
        X = x;
        Y = y;
        Amount = amount;
        UnitType = unitDefinitionSO.UnitType;
        Name = unitDefinitionSO.Name;
        IsPlayer = isPlayer;
    }

    public void ReceiveDamage(int damage)
    {
        int prev = UnitStats.Health;
        UnitStats.LastDamageAmount = damage;
        UnitStats.Health = Mathf.Max(UnitStats.Health - damage, 0);
        OnHealthChanged?.Invoke(UnitStats.Health);
        OnHit?.Invoke();

        if (UnitStats.Health == 0)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        UnitStats.Health = Mathf.Min(UnitStats.Health + amount, UnitStats.MaxHealth);
        OnHealthChanged?.Invoke(UnitStats.Health);
    }

    public void AddMaxHP(int amount)
    {
        UnitStats.MaxHealth += amount;
    }

    internal void TriggerAttack() => OnAttack?.Invoke();

    public override string ToString()
    {
        return $"{Name} + ({Amount})";
    }

    public string GetDescription()
    {
        return ToString();
    }
}
