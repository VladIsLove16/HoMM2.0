using System;
using System.Collections.Generic;
using UnityEditor;

public interface IDamagable : IGridContent
{
    void RecieveDamage(DamageContext context);
    void SimulateRecieveDamage(DamageContext context);
}