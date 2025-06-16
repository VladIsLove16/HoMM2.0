using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
//IBlockable, IEffectable, IAttackable,
public class UnitModel : IGridContent,  IDamageSource, IDescriptable, IDamageable, IEffectable, IEffectApplier
{
    public event Action<DamageContext> OnBeforeDealDamage;
    public event Action<DamageContext> OnBeforeTakeDamage;
    public event Action<int> OnHealthChanged;
    public event Action OnTurnStart;
    public event Action OnDeath;
    public event Action OnAttack;
    public event Action OnHit;

    public UnitStats Stats { get; }
    public string Name = string.Empty;
    public StatusEffectManager StatusEffectManager { get; }

    public UnitModel(UnitDataSO data, int amount)
    {
        Stats = new UnitStats(data, amount);
        StatusEffectManager = new StatusEffectManager(data.StartingEffects, this);
        Name = data.name;
    }

    public void ReceiveDamage(int damage)
    {
        int prev = Stats.Health;
        Stats.LastDamageAmount = damage;
        Stats.Health = Mathf.Max(Stats.Health - damage, 0);
        OnHealthChanged?.Invoke(Stats.Health);
        OnHit?.Invoke();

        if (Stats.Health == 0)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        Stats.Health = Mathf.Min(Stats.Health + amount, Stats.MaxHealth);
        OnHealthChanged?.Invoke(Stats.Health);
    }

    public void AddMaxHP(int amount)
    {
        Stats.MaxHealth += amount;
    }

    internal void TriggerAttack() => OnAttack?.Invoke();

    public string GetDescription()
    {
        return $"{Name} + ({Stats.Amount})";
    }
}
