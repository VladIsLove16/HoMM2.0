using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Данные для создания разнообразных эффектов
/// </summary>
[CreateAssetMenu(menuName = "Magic/Status Effect data")]
public class StatusEffectData : ScriptableObject
{
    [Tooltip("Имя эффекта")]
    public string EffectName;
    [SerializeField] private Sprite sprite;
    public Sprite Sprite => sprite;


    public List<ExpirationConditionBase> ExpirationConditions;

    [Tooltip("Реакции, которые нужно выполнить при событиях")]
    public List<EffectReactionBase> OnApply;
    public List<EffectReactionBase> OnTurnStart;
    public List<EffectReactionBase> OnTurnEnd;
    public List<DamageReactionBase> OnInDamage;
    public List<DamageReactionBase> OnOutDamage;
    public List<EffectReactionBase> OnRemove;

}
