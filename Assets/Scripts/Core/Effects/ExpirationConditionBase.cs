using System;
using UnityEngine;

public abstract class ExpirationConditionBase : ScriptableObject
{
    /// <summary>Создать копию для рантайма с собственным состоянием</summary>
    public abstract ExpirationConditionBase CreateRuntimeInstance();
    public abstract void Initialize();           // инициализация состояния
    public abstract void OnApply();            // при получении эффекта
    public abstract void OnTurnStarted();       // ход начат    
    public abstract void OnTurnEnded();       // ход закончен      
    public abstract void OnInDamage(DamageContext ctx);
    public abstract void OnOutDamage(DamageContext ctx);
    public abstract bool ShouldRemove { get; }    // признак удаления
    public abstract string GetRemaining();       // для UI отображения остатка
}
