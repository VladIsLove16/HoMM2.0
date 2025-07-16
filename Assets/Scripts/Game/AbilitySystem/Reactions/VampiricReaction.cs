// VampiricReaction.cs
using System;
using UnityEngine;
// ArmorReaction.cs

[CreateAssetMenu(menuName = "Magic/Reactions/VampiricReaction")]
public class VampiricReaction : DamageReactionBase
{
    int vampirismPercent;

    public override void Execute(DamageContext ctx)
    {
        ctx.damageSource.UnitState.Health = Math.Min(ctx.damageSource.UnitState.MaxHealth, ctx.damageSource.UnitState.Health + ctx.DamageAmount*vampirismPercent);
    }
}
