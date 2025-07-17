using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Данные для создания эффекта
/// </summary>
[CreateAssetMenu(menuName = "Magic/Status Effect Data")]
public class StatusEffectData : ScriptableObject
{
    [Tooltip("Имя эффекта")]
    public StatusEffectType Type;
    public string EffectName => Type.ToString();

    public List<ExpirationConditionBase> ExpirationConditions;  // list of policies

    [Tooltip("Реакции, которые нужно выполнить при применении эффекта")]
    public List<EffectReactionBase> OnApply;
    [Tooltip("Реакции, которые нужно выполнить при начале хода")]
    public List<EffectReactionBase> OnTurnStart;
    [Tooltip("Реакции, которые нужно выполнить при получении урона")]
    public List<DamageReactionBase> OnInDamage;
    [Tooltip("Реакции, которые нужно выполнить при нанесении урона")]
    public List<DamageReactionBase> OnOutDamage;
    [Tooltip("Реакции, которые нужно выполнить при избавлении от эффекта")]
    public List<EffectReactionBase> OnRemove;
}
