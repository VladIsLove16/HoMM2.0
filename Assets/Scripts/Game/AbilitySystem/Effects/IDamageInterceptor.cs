
// IDamageInterceptor.cs
/// <summary>
/// Перехватывает и модифицирует урон перед применением.
/// </summary>
public interface IDamageInterceptor
{
    void OnBeforeDamage(DamageContext ctx);
}
