using System;
using System.Collections.Generic;
using UnityEditor;

public interface IDamagable : IGridContent
{
    void ReceiveDamage(DamageContext context);
}