using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.Units
{
    public static class TestDataFactory
    {
        public static IReadOnlyDictionary<UnitType, UnitDefinitionSO> CreateSingleUnitData(UnitType type, int moveSpeed = 3, int health = 100)
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.MoveSpeed = moveSpeed;
            stats.Health = health;
            stats.MaxHealth = health;
            stats.InvulnerableEffects = new System.Collections.Generic.List<StatusEffectType>();

            var def = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            def.UnitType = type;
            def.Stats = stats;

            var dict = new Dictionary<UnitType, UnitDefinitionSO>();
            dict[type] = def;
            return dict;
        }
    }

}