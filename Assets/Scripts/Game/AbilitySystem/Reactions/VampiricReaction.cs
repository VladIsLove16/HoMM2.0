// VampiricReaction.cs
using System;
using UnityEngine;
// ArmorReaction.cs

[CreateAssetMenu(menuName = "Magic/Reactions/VampiricReaction")]
public class VampiricReaction : DamageReactionBase
{
    int vampirismPercent;

    public override void Execute(DamageContext ctx, bool simulation = false)
    {
        if(ctx.Source is UnitModel model)
        {
            model.ModifiedStats.Health = Math.Min(model.ModifiedStats.MaxHealth, model.ModifiedStats.Health + ctx.DamageAmount * vampirismPercent);
        }
    }
}
