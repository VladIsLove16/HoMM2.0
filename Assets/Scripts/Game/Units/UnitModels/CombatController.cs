using static Unity.VisualScripting.Member;

public class CombatController
{
    ///// <summary>Нанести чистый физический/магический урон.</summary>
    //public void DealDamage(IDamageable target, int amount, IDamageSource source)
    //{
    //    var ctx = new DamageContext(target, amount, source);
    //    source.UnitStats.StatusEffectManager.ApplyOnOut(ctx);
    //    if (target is IEffectableLegacy receiver)
    //        receiver.StatusEffectManager.ApplyOnIn(ctx);
    //    target.ReceiveDamage(ctx.Amount);
    //}

    ///// <summary>Наложить один статус-эффект.</summary>
    //public void ApplyStatusEffect(IEffectableLegacy target, StatusEffect effect)
    //{
    //    target.StatusEffectManager.Apply(effect);
    //}
}
