using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditMode.MushroomBook
{
    internal static class MushroomBookTestHelpers
    {

        public static UnitStats CreateStats(int health, int maxHealth, int damage, int moveSpeed, IList<UnityEngine.Object> tracker)
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = health;
            stats.MaxHealth = maxHealth;
            stats.Damage = damage;
            stats.MoveSpeed = moveSpeed;
            tracker?.Add(stats);
            return stats;
        }

        public static Sprite CreateTestSprite(IList<UnityEngine.Object> tracker)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            tracker?.Add(texture);
            tracker?.Add(sprite);
            return sprite;
        }

        public static TextMeshProUGUI CreateText(IList<UnityEngine.Object> tracker, string name, Transform parent)
        {
            var go = new GameObject(name);
            tracker?.Add(go);
            go.transform.SetParent(parent, false);
            return go.AddComponent<TextMeshProUGUI>();
        }

        public static Image CreateImage(IList<UnityEngine.Object> tracker, string name, Transform parent)
        {
            var go = new GameObject(name);
            tracker?.Add(go);
            go.transform.SetParent(parent, false);
            return go.AddComponent<Image>();
        }

        public static Transform CreateContainer(IList<UnityEngine.Object> tracker, string name, Transform parent)
        {
            var go = new GameObject(name);
            tracker?.Add(go);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        public static SerializedProperty RequireProperty(SerializedObject serializedObject, string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"{serializedObject.targetObject.GetType().Name}.{propertyName} property was not found.");

            return property;
        }
    }
}
