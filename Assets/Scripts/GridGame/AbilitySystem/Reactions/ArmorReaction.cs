// ArmorReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/ArmorReaction")]
public class ArmorReaction : DamageReactionBase
{
    [SerializeField] int armor;
    public ArmorReaction(int armor)
    {
        this.armor = armor;
    }

    public override void Execute(DamageContext ctx, bool simulation = false)
    {
        ctx.DamageAmount -= armor;
    }
}
