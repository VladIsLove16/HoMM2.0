using System.Collections.Generic;

public class DamageContext
{
    public int DamageAmount;
    public DamageType Type;
    public IDamageSource Source;
    public IDamagable Target { get; set; }
    public int DieAmount;
    public IReadOnlyList<StatusEffect> AppliedEffects => _appliedEffects;
    private List<StatusEffect> _appliedEffects = new();
    public DamageContext(int damageAmount, DamageType damageType, IDamageSource damageSource)
    {
        DamageAmount = damageAmount;
        this.Type = damageType;
        this.Source = damageSource;
    }
    public void AddDamageEffect(StatusEffect effect)
    {
        _appliedEffects.Add(effect);
    }
}

