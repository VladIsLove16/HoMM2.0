using System;
using System.Collections.Generic;

public class StatusEffectManager
{
    private readonly List<StatusEffect> effects = new();
    public IReadOnlyList<StatusEffect> ActiveEffects => effects;
    public void HandleOutDamage(DamageContext ctx, bool simulation = false)
    {
        if (ctx == null) return;
        effects.ForEach(e => e.HandleOutDamage(ctx, simulation));
    }
    public void HandleInDamage(DamageContext ctx, bool simulation = false)
    {
        if (ctx == null) return;
        effects.ForEach(e => e.HandleInDamage(ctx, simulation));
    }
    public void HandleTurnStart()
        => effects.ForEach(e => e.HandleTurnStart());
    public void Apply(StatusEffect effect)
    {
        if (effect == null) return;
        effects.Add(effect);
        effect.HandleApply();
    }
    public void Remove(StatusEffect effect)
    {
        if (effect == null) return;
        effects.Remove(effect);
        effect.HandleRemove();
    }

    public void HandleTurnEnd()
    {
        effects.ForEach(e => e.HandleTurnEnd());
    }
}