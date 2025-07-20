using UnityEngine;

public abstract class DamageReactionBase : ScriptableObject
{
    public abstract void Execute(DamageContext context, bool simulation = false);
}