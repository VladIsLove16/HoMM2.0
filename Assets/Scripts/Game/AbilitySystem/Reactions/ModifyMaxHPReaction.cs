// ModifyMaxHPReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Modify Max HP")]
public class ModifyMaxHPReaction : EffectReactionBase
{
    [Tooltip("Изменение MaxHP")]
    public int Amount;

    public override void Execute(EffectContext context)
    {
        if(context.Target is UnitModel model)
            model.AddMaxHP(Amount);
    }
}
