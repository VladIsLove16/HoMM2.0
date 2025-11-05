using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Increase Incoming")]
public class IncreaseIncomingDamageReaction : DamageReactionBase
{
    [Tooltip("Процент увеличения входящего урона")]
    public float PercentBonus;

    public override void Execute(DamageContext ctx, bool simulation = false)
    {
        ctx.DamageAmount = Mathf.CeilToInt(ctx.DamageAmount * (1 + PercentBonus / 100f));
    }
}