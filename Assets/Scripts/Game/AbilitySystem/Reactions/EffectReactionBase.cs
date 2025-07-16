// EffectReactionBase.cs
using UnityEngine;

public abstract class EffectReactionBase : ScriptableObject
{
    public abstract void Execute(EffectReactionContext context);
}
