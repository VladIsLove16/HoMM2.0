using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Данные для создания разнообразных эффектов
/// </summary>
[CreateAssetMenu(menuName = "Magic/Status Effect Data")]
public class StatusEffectData : ScriptableObject
{
    [Tooltip("Имя эффекта")]
    public string EffectName;

    public List<ExpirationConditionBase> ExpirationConditions;  // list of policies

    [Tooltip("Реакции, которые нужно выполнить при событиях")]
    public List<EffectReactionBase> OnApply;
    public List<EffectReactionBase> OnTurnStart;
    public List<DamageReactionBase> OnInDamage;
    public List<DamageReactionBase> OnOutDamage;
    public List<EffectReactionBase> OnRemove;
}
