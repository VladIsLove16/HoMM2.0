// EffectContext.cs
public struct EffectContext
{
    public readonly IEffectable Target;
    public readonly IEffectApplier Source;
    public readonly StatusEffectData Data;

    public EffectContext(IEffectable target, IEffectApplier source, StatusEffectData data)
    {
        Target = target;
        Source = source;
        Data = data;
    }
}