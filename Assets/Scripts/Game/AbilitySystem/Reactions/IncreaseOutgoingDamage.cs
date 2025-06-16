using UnityEngine;
// IncreaseOutgoingDamage.cs
[CreateAssetMenu(menuName = "Magic/Reactions/Increase Outgoing")]
public class IncreaseOutgoingDamage : DamageReactionBase
{
    [Tooltip("Процент прибавки к урону")]
    [Range(0, 100)] public float PercentBonus;

    public override void Execute(DamageContext ctx)
    {
        ctx.Amount = Mathf.CeilToInt(ctx.Amount * (1 + PercentBonus / 100f));
    }
}
