// UnitViewModel.cs
using System.Collections.Generic;
// Model/StatusEffectManager.cs
public class StatusEffectManager
{
    private readonly List<StatusEffect> effects = new();
    private readonly UnitModel owner;
   public  IReadOnlyList<StatusEffect> Effects => effects;

    public StatusEffectManager(IEnumerable<StatusEffectData> datas, UnitModel owner)
    {
        this.owner = owner;
        foreach (var d in datas)
            effects.Add(new StatusEffect(d, owner));
    }

    public void ApplyOnOut(DamageContext ctx)
        => effects.ForEach(e => e.OnOut(ctx));

    public void ApplyOnIn(DamageContext ctx)
        => effects.ForEach(e => e.OnIn(ctx));

    public void Add(StatusEffect effect) => effects.Add(effect);
    public void Remove(StatusEffect effect) => effects.Remove(effect);
}
