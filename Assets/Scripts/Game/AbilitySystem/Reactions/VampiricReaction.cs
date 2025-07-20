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
        //ctx.Source.UnitState.Health = Math.Min(ctx.Source.UnitState.MaxHealth, ctx.Source.UnitState.Health + ctx.DamageAmount*vampirismPercent);
    }
}
