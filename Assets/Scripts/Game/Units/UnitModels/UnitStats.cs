using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
[CreateAssetMenu(menuName = "Units/new UnitStats")]
public class UnitStats : ScriptableObject
{
    public   UnitType UnitType;
    public   int   Health ;
    public   int   MaxHealth ;
    public   int   Damage ;
    public   int   SpellPower ;
    public   int   Offense ;
    public   int   Defense ;
    public   int   MoveSpeed ;
    public   int   AttackRange ;
    public bool CanFly;
    public List<StatusEffectType> StartingEffects;
    public List<StatusEffectType> InvulnerableEffects;
}