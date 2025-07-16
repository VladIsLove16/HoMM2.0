public class DamageContext
{
    public int DamageAmount;
    public DamageType damageType;
    public IDamageSource damageSource;
    public DamageContext(int damageAmount, DamageType damageType, IDamageSource damageSource)
    {
        DamageAmount = damageAmount;
        this.damageType = damageType;
        this.damageSource = damageSource;
    }
}
