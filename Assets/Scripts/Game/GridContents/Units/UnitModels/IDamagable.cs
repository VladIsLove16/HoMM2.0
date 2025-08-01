using System;
using System.Collections.Generic;
using UnityEditor;

public interface IDamagable : IGridContent
{
    public bool IsBlueTeam {  get;}  
    void RecieveDamage(DamageContext context);
    void SimulateRecieveDamage(DamageContext context);
}