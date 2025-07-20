// HealReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Heal")]
public class HealReaction : EffectReactionBase
{
    [Tooltip("Исцеление на каждый триггер")]
    public int Amount;

    public override void Execute(EffectReactionContext context)
    {
        //context.Target.UnitState.Health = Mathf.Min(context.Target.UnitState.MaxHealth, context.Target.UnitState.Health + Amount);
    }
}
