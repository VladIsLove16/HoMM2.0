using System;

public interface IDamagable : IGridContent
{
    bool IsBlueTeam { get; }
   public void RecieveDamage(DamageContext context);
    public void SimulateRecieveDamage(DamageContext context);
}