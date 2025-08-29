/// <summary>
/// Перехватывает и модифицирует урон перед применением.
/// </summary>
public class EffectReactionContext
{
    public readonly IEffectable Target;
    public readonly IDamageSource Source;
    public EffectReactionContext(IEffectable target, IDamageSource source)
    {
        Source = source;
        Target = target;
    }
}
