// BlademailReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/BlademailReaction")]
public class BlademailReaction : DamageReactionBase
{
    int BlademailPercent;

    public override void Execute(DamageContext ctx, bool simulation = false)
    {
        if (ctx.Source is IDamagable damagable)
        {
            if(!simulation)
            {
                var сtx = new DamageContext(ctx.DamageAmount * BlademailPercent, DamageType.pure, ctx.Source);
                damagable.RecieveDamage(сtx);
            }
        }
    }
}