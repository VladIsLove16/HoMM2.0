public class DamageContext
{
    public int DamageAmount;
    public DamageType Type;
    public IDamageSource Source;
    public DamageContext(int damageAmount, DamageType damageType, IDamageSource damageSource)
    {
        DamageAmount = damageAmount;
        this.Type = damageType;
        this.Source = damageSource;
    }
}
