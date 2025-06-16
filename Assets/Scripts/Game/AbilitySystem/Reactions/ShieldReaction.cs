// ShieldReaction.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Magic/Reactions/Shield")]
public class ShieldReaction : DamageReactionBase 
{
    [Tooltip("Сколько единиц щита даётся при применении")]

    // локальное поле, хранит остаток щита
    private int remainingShield;

    public override void Execute(DamageContext dmgCtx)
    {
        if (remainingShield <= 0) return;

        int absorbed = Mathf.Min(remainingShield, dmgCtx.Amount);
        dmgCtx.Amount -= absorbed;
        remainingShield -= absorbed;
    }
}
