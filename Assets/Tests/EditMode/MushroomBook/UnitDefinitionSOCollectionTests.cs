using Adventure.Integration.Battle;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tests.EditMode.MushroomBook
{
    [TestFixture]
    public class UnitDefinitionSOCollectionTests
    {
        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        [Test]
        public void TryGet_ReturnsDefinition_WhenPresent()
        {
            var definition = CreateDefinition(UnitType.Witch);
            var collection = CreateCollection(definition);

            Assert.That(collection.TryGet(UnitType.Witch, out var result), Is.True);
            Assert.That(result, Is.SameAs(definition));
        }

        [Test]
        public void GetAll_ReturnsSerializedItems()
        {
            var first = CreateDefinition(UnitType.Witch);
            var second = CreateDefinition(UnitType.Warrok);
            var collection = CreateCollection(first, second);

            var all = collection.GetAll();

            CollectionAssert.AreEqual(new[] { first, second }, all);
        }

        private UnitDefinitionSOCollection CreateCollection(params UnitDefinitionSO[] definitions)
        {
            var collection = ScriptableObject.CreateInstance<UnitDefinitionSOCollection>();
            _created.Add(collection);

            var serialized = new SerializedObject(collection);
            var items = serialized.FindProperty("unitDefinitionSOs");
            items.arraySize = definitions.Length;

            for (int i = 0; i < definitions.Length; i++)
            {
                items.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }

            serialized.ApplyModifiedProperties();
            return collection;
        }

        private UnitDefinitionSO CreateDefinition(UnitType type)
        {
            var definition = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            definition.UnitType = type;
            definition.name = $"{type}_Definition";
            _created.Add(definition);
            return definition;
        }
    }
}
