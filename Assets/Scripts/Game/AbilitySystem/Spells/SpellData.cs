using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Заклинание, применяющие эффекты или наносящее прямой урон
/// </summary>
[CreateAssetMenu(menuName = "Magic/Spell")]
public class SpellData : ScriptableObject
{
    public string SpellName;
    public int Damage;
    public List<StatusEffectData> StatusEffects;
    public SpellZoneType ZoneType;
}
public enum SpellZoneType { Cross, Quad, Circle, Custom }
