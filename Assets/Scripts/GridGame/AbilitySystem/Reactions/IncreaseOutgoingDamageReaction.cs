using UnityEngine;
// IncreaseOutgoingDamageReaction.cs
[CreateAssetMenu(menuName = "Magic/Reactions/Increase Outgoing")]
public class IncreaseOutgoingDamageReaction : DamageReactionBase
{
    [Tooltip("Процент прибавки к урону")]
    [Range(0, 100)] public float PercentBonus;

    public override void Execute(DamageContext ctx, bool simulation = false)
    {
        ctx.DamageAmount = Mathf.CeilToInt(ctx.DamageAmount * (1 + PercentBonus / 100f));
    }
}
