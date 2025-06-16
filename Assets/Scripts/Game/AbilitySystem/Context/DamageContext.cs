using UnityEditor.Experimental.GraphView;

/// <summary>
/// Перехватывает и модифицирует урон перед применением.
/// </summary>
public class DamageContext
{
    public int Amount;
    public readonly IDamageable Target;   // чтобы внутри реакций можно было отписаться
    public readonly IDamageSource Source;
    public DamageContext(IDamageable target,  int amount, IDamageSource source)
    {
        Amount = amount;
        Source = source;
        Target = target;
    }
}
