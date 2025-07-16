using System;
using System.Collections.Generic;
using UnityEditor;

public interface IDamagable
{
    void ReceiveDamage(DamageContext context);
    event Action<int> HealthChanged;
    event Action Died;
}
public interface IUnit : IDamagable
{
    UnitType Type { get; }
    UnitState Stats { get; }
    bool IsBlueTeam { get; }
    IReadOnlyList<StatusEffect> ActiveEffects { get; }

    event Action TurnStarted;
    event Action TurnEnded;
}