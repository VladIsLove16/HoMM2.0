public class DamageContext
{
    public int DamageAmount;
    public DamageType Type;
    public IDamageSource Source;
    public int DieAmount;
    public DamageContext(int damageAmount, DamageType damageType, IDamageSource damageSource)
    {
        DamageAmount = damageAmount;
        this.Type = damageType;
        this.Source = damageSource;
    }
}
public class AttackContext
{
    public IDamagable Target;
    public AttackContext(IDamagable target)
    {
        Target = target;
    }
}

