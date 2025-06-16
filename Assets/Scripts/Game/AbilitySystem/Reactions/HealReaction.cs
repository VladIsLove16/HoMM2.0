// HealReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Heal")]
public class HealReaction : EffectReactionBase
{
    [Tooltip("Исцеление на каждый триггер")]
    public int Amount;

    public override void Execute(EffectContext context)
    {
        if(context.Target is UnitModel unit)
            unit.Heal(Amount);
    }
}
