// AccelerateReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/AccelerateReaction")]
public class AccelerateReaction : EffectReactionBase
{
    [SerializeField] int accelerateAmount;
    public override void Execute(EffectReactionContext ctx)
    {
        //ctx.Target.UnitState.MoveSpeed+= accelerateAmount; 
    }
}
