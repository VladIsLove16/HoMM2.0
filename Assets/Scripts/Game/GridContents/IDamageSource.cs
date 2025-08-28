public interface IDamageSource
{
    public DamageContext SendDamage(AttackContext ctx);
    public DamageContext SimulateSendDamage(AttackContext ctx);
}