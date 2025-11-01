using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitModelFactory_EditModeTests
    {
        [Test]
        public void Create_WithProvidedDictionary_CreatesUnitModel()
        {
            // Arrange - create SO at runtime
            var def = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            def.UnitType = UnitType.Archer;
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 10; stats.MaxHealth = 10; stats.Damage = 2;
            def.UnitStats = stats;

            var dict = new Dictionary<UnitType, UnitDefinitionSO>() { { UnitType.Archer, def } };
            var factory = new UnitModelFactory(dict);

            var spawn = new UnitSpawnParams( 1,  2, UnitType.Archer,1,true);

            // Act
            var model = factory.Create(spawn);

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(spawn.UnitType, model.UnitType.Value);
            Assert.AreEqual(new Vector2Int(spawn.X, spawn.Y), model.Position.Value);
        }

        [Test]
        public void UnitDefinitionSO_SetMaterialProvider_AllowsTestProvider()
        {
            var def = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            var provider = new TestMaterialProvider();
            def.SetMaterialProvider(provider);

            Assert.AreEqual(provider.GetBlueTeamMaterial().name, def.GetBlueTeamMaterial().name);
        }

        private class TestMaterialProvider : IMaterialProvider
        {
            private Material _blue = new Material(Shader.Find("Standard")) { name = "blue" };
            private Material _hoveredBlue = new Material(Shader.Find("Standard")) { name = "hoveredBlue" };
            private Material _red = new Material(Shader.Find("Standard")) { name = "red" };
            private Material _hoveredRed = new Material(Shader.Find("Standard")) { name = "hoveredRed" };

            public Material GetTeamMaterial(Team team) => team == Team.Blue ? _blue : _red;
            public Material GetHoveredTeamMaterial(Team team) => team == Team.Blue ? _hoveredBlue : _hoveredRed;
            public Material GetBlueTeamMaterial() => _blue;
            public Material GetHoveredBlueTeamMaterial() => _hoveredBlue;
            public Material GetRedTeamMaterial() => _red;
            public Material GetHoveredRedTeamMaterial() => _hoveredRed;
        }
    }
}
