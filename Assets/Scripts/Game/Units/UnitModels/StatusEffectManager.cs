using System;
using System.Collections.Generic;

public class StatusEffectManager
{
    private readonly List<StatusEffect> effects = new();
    public IReadOnlyList<StatusEffect> ActiveEffects => effects;
    public void HandleOutDamage(DamageContext ctx, bool simulation = false)
        => effects.ForEach(e => e.HandleOutDamage(ctx));
    public void HandleInDamage(DamageContext ctx, bool simulation = false)
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

    public void HandleTurnEnd()
    {
        effects.ForEach(e => e.HandleTurnEnd());
    }
}