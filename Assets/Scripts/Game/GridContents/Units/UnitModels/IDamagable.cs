using System;

public interface IDamagable : IGridContent
{
    bool IsBlueTeam { get; }
    void RecieveDamage(DamageContext context);
    void SimulateRecieveDamage(DamageContext context);
}