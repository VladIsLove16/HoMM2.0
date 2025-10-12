using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Заклинание, применяющие эффекты или наносящее прямой урон
/// </summary>
[CreateAssetMenu(menuName = "Magic/Spell")]
public class SpellData : ScriptableObject
{
    public SpellType SpellName;
    public int Damage;
    public DamageType DamageType;
    public List<StatusEffectData> StatusEffects;
    public SpellZoneType ZoneType;
}
public enum SpellZoneType { Cross, Quad, Circle, Target, Custom } 
public enum SpellType { None,Fireball, MagicMissle}
