// ModifyMaxHPReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/StatModifier")]
public class StatModifierEffectReaction : StatModifierEffectReactionBase
{
    public override void Execute(EffectReactionContext context)
    {
        IEffectable target = context.Target;
        target.ModifiedStats += StatModifier;
    }
}
