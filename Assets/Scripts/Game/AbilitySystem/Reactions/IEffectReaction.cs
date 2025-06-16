// IEffectReaction.cs
// IEffectReaction.cs
public interface IEffectReaction
{
    /// <summary>
    /// Выполнить реакцию:
    /// контекст содержит источник, цель и оставшиеся ходы
    /// </summary>
    void Execute(EffectContext context);
}
