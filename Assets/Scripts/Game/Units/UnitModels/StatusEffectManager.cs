// UnitViewModel.cs
using System.Collections.Generic;
// Model/StatusEffectManager.cs
public class StatusEffectManager
{
    private readonly List<StatusEffect> effects = new();
    public  IReadOnlyList<StatusEffect> ActiveEffects => effects;
    public void HandleOutDamage(DamageContext ctx)
        => effects.ForEach(e => e.HandleOutDamage(ctx));
    public void HandleInDamage(DamageContext ctx)
        => effects.ForEach(e => e.HandleInDamage(ctx));
    public void HandleTurnStart()
        => effects.ForEach(e => e.HandleTurnStart());
    public void Apply(StatusEffect effect)
    {
        effects.Add(effect);
        effect.HandleApply();
    }
    public void Remove(StatusEffect effect)
    {
        effects.Remove(effect);
        effect.HandleRemove();
    }
}
