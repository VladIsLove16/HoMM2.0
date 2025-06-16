// DamageReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Damage")]
public class DamageReaction : DamageReactionBase
{
    public int Amount;

    public override void Execute(DamageContext context)
    {
        context.Amount += Amount;
    }
}
