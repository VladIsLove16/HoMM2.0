using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Debug = UnityEngine.Debug;
namespace Tests.EditMode.GridContents.Units
{
        internal class MockStatusEffect : StatusEffect
        {
            public MockStatusEffect(StatusEffectType type) 
                : base(FindExistingStatusEffectData(type), new MockEffectable(), new MockEffectApplier()) { }

            private static StatusEffectData FindExistingStatusEffectData(StatusEffectType type)
            {
                // Ищем существующие StatusEffectData в проекте через AssetDatabase
                var guids = UnityEditor.AssetDatabase.FindAssets("t:StatusEffectData");
                var allEffects = new List<StatusEffectData>();
                
                foreach (var guid in guids)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<StatusEffectData>(path);
                    if (asset != null)
                    {
                        allEffects.Add(asset);
                    }
                }

                // Сначала пытаемся найти по типу
                var existing = allEffects.FirstOrDefault(se => se.Type == type);
                if (existing != null)
                {
                    return existing;
                }

                // Fallback: берем любой существующий StatusEffectData
                var fallback = allEffects.FirstOrDefault();
                if (fallback != null)
                {
                    Debug.LogWarning($"Using fallback StatusEffectData for type {type}. Found {allEffects.Count} total effects in project.");
                    return fallback;
                }

                // Если ничего не найдено, создаем минимальный для тестов
                Debug.LogError($"No StatusEffectData found in project");
                throw new InvalidOperationException($"No StatusEffectData found in project for testing");
            }
        } // Mock классы для тестирования
        internal class MockDamagable : IDamagable
        {
            public bool IsBlueTeam => true;
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;

            public void RecieveDamage(DamageContext ctx) { }
            public void SimulateRecieveDamage(DamageContext ctx) { }
        }
        internal class MockEffectable : IEffectable
        {
            public bool IsBlueTeam => true;
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;
            public UnitStats ModifiedStats { get; set; }

            public void RecieveDamage(DamageContext ctx) { }
            public void SimulateRecieveDamage(DamageContext ctx) { }
            public void ApplyEffect(StatusEffect statusEffect) { }
            public void RemoveEffect(StatusEffect statusEffect) { }
    }
        internal class MockEffectApplier : IEffectApplier
        {
            public void RecieveDamage(DamageContext ctx)
            {
            }

            public DamageContext SendDamage(AttackContext ctx)
            {
                throw new NotImplementedException();
            }

            public void SimulateRecieveDamage(DamageContext ctx) { }

            public DamageContext SimulateSendDamage(AttackContext ctx)
            {
                throw new NotImplementedException();
            }

            IReadOnlyList<StatusEffect> IEffectApplier.GetAppliedEffects()
            {
                throw new NotImplementedException();
        }
        }
    }

