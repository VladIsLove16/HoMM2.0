public class DamageContextPreview
{
    public DamageContext Damage { get; internal set; }
    public DamageContextPreview(DamageContext context)
    {
        Damage = context;
    }
}