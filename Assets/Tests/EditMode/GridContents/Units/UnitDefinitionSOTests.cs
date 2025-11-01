using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitDefinitionSOTests
    {
        private UnitDefinitionSO _unitDefinition;
        private MockMaterialProvider _mockMaterialProvider;

        [SetUp]
        public void SetUp()
        {
            _unitDefinition = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            _mockMaterialProvider = new MockMaterialProvider();
            
            // Устанавливаем базовые параметры
            _unitDefinition.UnitType = UnitType.Archer;
            _unitDefinition.DisplayName = "Test Archer";
            
            // Создаем тестовые характеристики
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 100;
            stats.MaxHealth = 100;
            stats.Damage = 25;
            _unitDefinition.UnitStats = stats;
        }

        [TearDown]
        public void TearDown()
        {
            if (_unitDefinition != null)
            {
                if (_unitDefinition.UnitStats != null)
                {
                    UnityEngine.Object.DestroyImmediate(_unitDefinition.UnitStats);
                }
                UnityEngine.Object.DestroyImmediate(_unitDefinition);
            }
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly()
        {
            // Assert
            Assert.That(_unitDefinition.UnitType, Is.EqualTo(UnitType.Archer));
            Assert.That(_unitDefinition.DisplayName, Is.EqualTo("Test Archer"));
            Assert.That(_unitDefinition.UnitStats, Is.Not.Null);
            Assert.That(_unitDefinition.UnitStats.Health, Is.EqualTo(100));
        }

        [Test]
        public void GetBlueTeamMaterial_WithMockProvider_ReturnsNotNull()
        {
            // Arrange
            _unitDefinition.SetMaterialProvider(_mockMaterialProvider);

            // Act
            var material = _unitDefinition.GetBlueTeamMaterial();

            // Assert
            Assert.That(material, Is.Not.Null);
            Assert.That(material.name, Is.EqualTo("Mock Blue Team Material"));
        }

        [Test]
        public void GetRedTeamMaterial_WithMockProvider_ReturnsNotNull()
        {
            // Arrange
            _unitDefinition.SetMaterialProvider(_mockMaterialProvider);

            // Act
            var material = _unitDefinition.GetRedTeamMaterial();

            // Assert
            Assert.That(material, Is.Not.Null);
            Assert.That(material.name, Is.EqualTo("Mock Red Team Material"));
        }

        [Test]
        public void GetHoveredBlueTeamMaterial_WithMockProvider_ReturnsNotNull()
        {
            // Arrange
            _unitDefinition.SetMaterialProvider(_mockMaterialProvider);

            // Act
            var material = _unitDefinition.GetHoveredBlueTeamMaterial();

            // Assert
            Assert.That(material, Is.Not.Null);
            Assert.That(material.name, Is.EqualTo("Mock Hovered Blue Team Material"));
        }

        [Test]
        public void GetHoveredRedTeamMaterial_WithMockProvider_ReturnsNotNull()
        {
            // Arrange
            _unitDefinition.SetMaterialProvider(_mockMaterialProvider);

            // Act
            var material = _unitDefinition.GetHoveredRedTeamMaterial();

            // Assert
            Assert.That(material, Is.Not.Null);
            Assert.That(material.name, Is.EqualTo("Mock Hovered Red Team Material"));
        }

        [Test]
        public void StartingEffects_InitiallyEmpty_ReturnsEmptyList()
        {
            // Assert
            Assert.That(_unitDefinition.StartingEffects, Is.Not.Null);
            Assert.That(_unitDefinition.StartingEffects.Count, Is.EqualTo(0));
        }

        [Test]
        public void InvulnerableEffects_InitiallyEmpty_ReturnsEmptyList()
        {
            // Assert
            Assert.That(_unitDefinition.InvulnerableEffects, Is.Not.Null);
            Assert.That(_unitDefinition.InvulnerableEffects.Count, Is.EqualTo(0));
        }

        [Test]
        public void SetMaterialProvider_WithValidProvider_UpdatesProvider()
        {
            // Arrange
            var newMockProvider = new MockMaterialProvider();

            // Act
            _unitDefinition.SetMaterialProvider(newMockProvider);

            // Assert
            var material = _unitDefinition.GetBlueTeamMaterial();
            Assert.That(material.name, Is.EqualTo("Mock Blue Team Material"));
        }

        [Test]
        public void SetMaterialProvider_WithNullProvider_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _unitDefinition.SetMaterialProvider(null));
        }

        // Mock провайдер материалов для тестирования
        private class MockMaterialProvider : IMaterialProvider
        {
            private readonly Material _blueTeamMaterial;
            private readonly Material _hoveredBlueTeamMaterial;
            private readonly Material _redTeamMaterial;
            private readonly Material _hoveredRedTeamMaterial;

            public MockMaterialProvider()
            {
                // Создаем мок материалы без загрузки из ресурсов
                _blueTeamMaterial = CreateMockMaterial("Mock Blue Team Material", Color.blue);
                _hoveredBlueTeamMaterial = CreateMockMaterial("Mock Hovered Blue Team Material", Color.cyan);
                _redTeamMaterial = CreateMockMaterial("Mock Red Team Material", Color.red);
                _hoveredRedTeamMaterial = CreateMockMaterial("Mock Hovered Red Team Material", Color.magenta);
            }

            private Material CreateMockMaterial(string name, Color color)
            {
                var material = new Material(Shader.Find("Standard"));
                material.name = name;
                material.color = color;
                return material;
            }

            public Material GetBlueTeamMaterial() => _blueTeamMaterial;
            public Material GetHoveredBlueTeamMaterial() => _hoveredBlueTeamMaterial;
            public Material GetRedTeamMaterial() => _redTeamMaterial;
            public Material GetHoveredRedTeamMaterial() => _hoveredRedTeamMaterial;

            public Material GetTeamMaterial(Team team) => team == Team.Blue ? _blueTeamMaterial : _redTeamMaterial;

            public Material GetHoveredTeamMaterial(Team team) => team == Team.Blue ? _hoveredBlueTeamMaterial : _hoveredRedTeamMaterial;
        }
    }
}
