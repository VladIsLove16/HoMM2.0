using System;
public interface IEffectable : IGridContent, IDamageable
{
    public event Action<DamageContext> OnBeforeDealDamage;
    public event Action<DamageContext> OnBeforeTakeDamage;
    public event Action<int> OnHealthChanged;
    public event Action OnTurnStart;
    public event Action OnDeath;
    public StatusEffectManager StatusEffectManager { get; }
}