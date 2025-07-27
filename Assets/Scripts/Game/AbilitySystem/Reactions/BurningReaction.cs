// BurningReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Burning")]
public class BurningReaction : EffectReactionBase
{
    [Tooltip("Горение на каждый триггер")]
    public int Amount;
    public DamageType damageType = DamageType.magical;

    public override void Execute(EffectReactionContext context)
    {
        var ctx =  new DamageContext(Amount, damageType, context.Source);
        context.Target.RecieveDamage(ctx);
    }
}
