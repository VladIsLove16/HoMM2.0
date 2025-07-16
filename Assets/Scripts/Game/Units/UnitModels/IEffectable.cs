using System;
/// <summary>
/// Тот, кто может быть целью эффектов
/// </summary>
public interface IEffectable : IGridContent, IDamagable
{
    event Action<DamageContext> BeforeInDamage;
    event Action<DamageContext> BeforeOutDamage;
    public event Action TurnStarted;
    public event Action TurnEnded;
    public UnitState UnitState { get; }

    public void ApplyEffect(StatusEffectData statusEffect,IEffectApplier effectApplier);
    public void RemoveEffect(StatusEffect statusEffect);
}