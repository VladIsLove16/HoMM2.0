public class DamageContextPreview
{
    public static DamageContextPreview Empty { get; } = new DamageContextPreview(null);

    public DamageContext Damage { get; internal set; }

    public DamageContextPreview(DamageContext context)
    {
        Damage = context;
    }
}
