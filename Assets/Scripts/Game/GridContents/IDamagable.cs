using System;
using System.Collections.Generic;

public interface IDamagable : IGridContent
{
    public bool IsBlueTeam {  get;}  
    void RecieveDamage(DamageContext context);
    void SimulateRecieveDamage(DamageContext context);
}