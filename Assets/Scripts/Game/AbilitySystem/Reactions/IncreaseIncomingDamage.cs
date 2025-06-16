using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Increase Incoming")]
public class IncreaseIncomingDamage : DamageReactionBase
{
    [Tooltip("Процент увеличения входящего урона")]
    public float PercentBonus;

    public override void Execute(DamageContext ctx)
    {
        ctx.Amount = Mathf.CeilToInt(ctx.Amount * (1 + PercentBonus / 100f));
    }
}