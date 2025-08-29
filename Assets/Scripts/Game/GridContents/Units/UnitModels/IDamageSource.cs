public interface IDamageSource
{
    DamageContext SendDamage(AttackContext ctx);
    DamageContext SimulateSendDamage(AttackContext ctx);
}