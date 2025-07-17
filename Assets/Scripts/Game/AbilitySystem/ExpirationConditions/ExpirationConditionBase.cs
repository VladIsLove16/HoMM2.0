using UnityEngine;

public abstract class ExpirationConditionBase : ScriptableObject
{
    /// <summary>Создать копию для рантайма с собственным состоянием</summary>
    public abstract ExpirationConditionBase CreateRuntimeInstance();
    /// <summary>
    /// Инициализация состояния
    /// </summary>
    public abstract void Initialize();        
    /// <summary>
    /// При применении эффекта
    /// </summary>
    public abstract void OnApply();
    /// <summary>
    /// При старте хода
    /// </summary>
    public abstract void OnTurn();
    /// <summary>
    /// При получении урона
    /// </summary>
    public abstract void OnInDamage(DamageContext ctx);
    /// <summary>
    /// При нанесении урона
    /// </summary>
    public abstract void OnOutDamage(DamageContext ctx);
    /// <summary>
    /// Признак удаления
    /// </summary>
    public abstract bool ShouldRemove { get; }
    /// <summary>
    /// Количество оставшихся срабатываний
    /// </summary>
    public abstract string GetRemaining();       
}
