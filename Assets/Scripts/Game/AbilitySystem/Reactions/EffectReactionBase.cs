// EffectReactionBase.cs
using UnityEngine;

public abstract class EffectReactionBase : ScriptableObject, IEffectReaction
{
    public abstract void Execute(EffectContext context);
}
