using static Unity.VisualScripting.Member;

public class CombatController
{
    /// <summary>Нанести чистый физический/магический урон.</summary>
    public void DealDamage(IDamageable target, int amount, IDamageSource source)
    {
        var ctx = new DamageContext(target, amount, source);
        source.UnitStats.StatusEffectManager.ApplyOnOut(ctx);
        if(target is IEffectable receiver)
            receiver.StatusEffectManager.ApplyOnIn(ctx);
        target.ReceiveDamage(ctx.Amount);
    }

    /// <summary>Наложить один статус-эффект.</summary>
    public void ApplyStatusEffect(IEffectable target, StatusEffect effect)
    {
        target.StatusEffectManager.Add(effect);
    }
}
