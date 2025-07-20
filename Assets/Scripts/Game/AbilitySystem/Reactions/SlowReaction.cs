// SlowReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/SlowReactionReaction")]
public class SlowReaction : EffectReactionBase
{
    [SerializeField] int slowAmount;
    public override void Execute(EffectReactionContext ctx)
    {
        //ctx.Target.UnitState.MoveSpeed-=slowAmount; 
    }
}
