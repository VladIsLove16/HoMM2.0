using System.Collections.Generic;

public interface IEffectApplier : IDamageSource
{
    IReadOnlyList<StatusEffect> GetAppliedEffects();
}

