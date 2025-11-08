using NUnit.Framework;
using System.Collections.Generic;
using Tests.EditMode.ActionHandlers;
using UnityEditor;
using UnityEngine;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitModelFactory_EditModeTests
    {
        [Test]
        public void Create_WithProvidedDictionary_CreatesUnitModel()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 10;
            stats.MaxHealth = 10;
            stats.Damage = 2;
            var provider = new MockUnitStatsProviderInline();
            provider.SetData(UnitType.Archer, stats);
            var unitFactory = new UnitModelFactory(provider);
            var spawn = new UnitSpawnParams(1, 2, UnitType.Archer, 1, true);
            var model = unitFactory.Create(spawn);

            Assert.IsNotNull(model);
            Assert.AreEqual(spawn.UnitType, model.UnitType.Value);
            Assert.AreEqual(new Vector2Int(spawn.X, spawn.Y), model.Position.Value);

            Object.DestroyImmediate(stats);
        }
    }
}
