using System;
using System.Collections.Generic;
using System.Linq;
using Game.Achievements;
using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tests.EditMode.Achievements
{
    [TestFixture]
    public sealed class AchievementCatalogValidationTests
    {
        private const string ExpectedCatalogPath = "Assets/ScriptableObjects/Achievments/AchievementCatalog.asset";

        [Test]
        public void ExpectedCatalogAsset_Exists()
        {
            #if UNITY_EDITOR
            var catalog = AssetDatabase.LoadAssetAtPath<AchievementCatalog>(ExpectedCatalogPath);
            Assert.That(catalog, Is.Not.Null, $"Catalog not found at '{ExpectedCatalogPath}'.");
            #else
            Assert.Ignore("AssetDatabase is editor-only.");
            #endif
        }

        [Test]
        public void Catalogs_HaveValidDefinitions()
        {
            #if UNITY_EDITOR
            var catalogs = LoadAllCatalogs();
            Assert.That(catalogs.Count, Is.GreaterThan(0), "No AchievementCatalog assets found.");

            foreach (var catalog in catalogs)
            {
                Assert.That(catalog, Is.Not.Null);
                var entries = catalog.Achievements?.Where(a => a != null).ToList() ?? new List<AchievementDefinition>();
                Assert.That(entries.Count, Is.GreaterThan(0), $"Catalog '{catalog.name}' has no achievements.");

                AssertUniqueIds(entries, catalog.name);
                AssertDisplayText(entries, catalog.name);
                AssertConditions(entries, catalog.name);
            }
            #else
            Assert.Ignore("AssetDatabase is editor-only.");
            #endif
        }

        #if UNITY_EDITOR
        private static List<AchievementCatalog> LoadAllCatalogs()
        {
            var result = new List<AchievementCatalog>();
            foreach (var guid in AssetDatabase.FindAssets("t:AchievementCatalog"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<AchievementCatalog>(path);
                if (asset != null)
                    result.Add(asset);
            }
            return result;
        }
        #endif

        private static void AssertUniqueIds(IEnumerable<AchievementDefinition> entries, string catalogName)
        {
            var duplicates = entries
                .Where(e => e != null)
                .GroupBy(e => e.Id?.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => string.IsNullOrWhiteSpace(g.Key) || g.Count() > 1)
                .ToList();

            if (duplicates.Count == 0)
                return;

            var details = string.Join(", ", duplicates.Select(d => $"'{d.Key ?? "<empty>"}' x{d.Count()}"));
            Assert.Fail($"Catalog '{catalogName}' has invalid or duplicate ids: {details}");
        }

        private static void AssertDisplayText(IEnumerable<AchievementDefinition> entries, string catalogName)
        {
            var invalid = entries
                .Where(e => e != null)
                .Where(e => string.IsNullOrWhiteSpace(e.Title) && IsLocalizedEmpty(GetLocalizedTitle(e)))
                .Select(e => e.Id)
                .ToList();

            if (invalid.Count == 0)
                return;

            Assert.Fail($"Catalog '{catalogName}' has achievements without title/locale: {string.Join(", ", invalid)}");
        }

        private static void AssertConditions(IEnumerable<AchievementDefinition> entries, string catalogName)
        {
            var invalid = entries
                .Where(e => e != null)
                .Where(e => e.Conditions == null || e.Conditions.Count == 0 || e.Conditions.Any(c => c == null))
                .Select(e => e.Id)
                .ToList();

            if (invalid.Count == 0)
                return;

            Assert.Fail($"Catalog '{catalogName}' has achievements with empty/null conditions: {string.Join(", ", invalid)}");
        }

        private static object GetLocalizedTitle(AchievementDefinition definition)
        {
            var prop = typeof(AchievementDefinition).GetProperty("TitleLocalized", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return prop?.GetValue(definition);
        }

        private static bool IsLocalizedEmpty(object localized)
        {
            if (localized == null)
                return true;

            var type = localized.GetType();
            var isEmptyProp = type.GetProperty("IsEmpty", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (isEmptyProp != null && isEmptyProp.PropertyType == typeof(bool))
            {
                return (bool)isEmptyProp.GetValue(localized);
            }

            return true;
        }
    }
}
