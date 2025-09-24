using System;

public interface IDamagable : IGridContent
{
    public void RecieveDamage(DamageContext context);
    public void SimulateRecieveDamage(DamageContext context);
}