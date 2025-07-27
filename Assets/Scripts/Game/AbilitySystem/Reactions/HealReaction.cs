// HealReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Heal")]
public class HealReaction : EffectReactionBase
{
    [Tooltip("Исцеление на каждый триггер")]
    public int Amount;

    public override void Execute(EffectReactionContext context)
    {
        //context.TargetObject.UnitState.Health = Mathf.Min(context.TargetObject.UnitState.MaxHealth, context.TargetObject.UnitState.Health + Amount);
    }
}
