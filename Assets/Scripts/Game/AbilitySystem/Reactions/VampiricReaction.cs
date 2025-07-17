// VampiricReaction.cs
using UnityEngine;
// ArmorReaction.cs

[CreateAssetMenu(menuName = "Magic/Reactions/VampiricReaction")]
public class VampiricReaction : DamageReactionBase
{
    int vampisimPercent;
    public VampiricReaction(int vampisimPercent)
    {
        this.vampisimPercent = vampisimPercent;
    }

    public override void Execute(DamageContext ctx)
    {
        //if(ctx.Target is UnitModelLegacy unit)
        //    unit.Heal((int)vampisimPercent*ctx.Amount);
    }
}
