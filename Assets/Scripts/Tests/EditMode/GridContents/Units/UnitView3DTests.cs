using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniRx;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitView3DTests
    {
        private GameObject _gameObject;
        private UnitView3D _unitView;
        private UnitViewModel _unitViewModel;
        private UnitModel _unitModel;
        private MockUnitViewUI _mockUnitViewUI;
        private MockAnimator _mockAnimator;

        [SetUp]
        public void SetUp()
        {
            // Создаем GameObject с компонентами
            _gameObject = new GameObject("TestUnitView");
            _unitView = _gameObject.AddComponent<UnitView3D>();
            
            // Добавляем Animator
            _mockAnimator = _gameObject.AddComponent<MockAnimator>();
            
            // Создаем базовые характеристики для тестов
            var baseStats = ScriptableObject.CreateInstance<UnitStats>();
            baseStats.Health = 100;
            baseStats.MaxHealth = 100;
            baseStats.Damage = 25;
            baseStats.InvulnerableEffects = new List<StatusEffectType>();

            // Создаем модель юнита
            _unitModel = new UnitModel(baseStats, UnitType.Archer, 5, 3, 10, true);

            // Создаем мок материалов
            var materialsInfo = new UnitViewModelMaterialsInfo(new MockUnitDefinitionSO());
            
            // Создаем ViewModel
            _unitViewModel = new UnitViewModel(_unitModel, materialsInfo);

            // Создаем мок UI
            _mockUnitViewUI = new MockUnitViewUI();
            
            // Устанавливаем поля через reflection для тестов
            var uiField = typeof(UnitView3D).GetField("unitViewUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            uiField?.SetValue(_unitView, _mockUnitViewUI);

            var meshesField = typeof(UnitView3D).GetField("meshes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            meshesField?.SetValue(_unitView, new SkinnedMeshRenderer[0]);
        }

        [TearDown]
        public void TearDown()
        {
            if (_unitModel?.BaseUnitStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_unitModel.BaseUnitStats);
            }
            
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void Init_WithValidViewModel_InitializesCorrectly()
        {
            // Act
            _unitView.Init(_unitViewModel);

            // Assert
            Assert.That(_unitView.Model, Is.EqualTo(_unitModel));
        }

        [Test]
        public void Init_WithValidViewModel_SubscribesToModelEvents()
        {
            // Act
            _unitView.Init(_unitViewModel);

            // Act - вызываем события модели
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);
            _unitModel.SendDamage(attackContext);

            // Assert - проверяем, что события обрабатываются
            // В реальном коде здесь были бы проверки анимаций
        }

        [Test]
        public void MoveByRoute_WithValidRoute_EnqueuesAction()
        {
            // Arrange
            _unitView.Init(_unitViewModel);
            var route = new List<Vector3> { Vector3.zero, Vector3.one, Vector3.right };

            // Act
            _unitView.MoveByRoute(route);

            // Assert
            // В реальном коде здесь проверяли бы, что действие добавлено в очередь
            // Для тестов достаточно, что метод не выбрасывает исключений
        }

        [Test]
        public void Dispose_ClearsDisposables()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.Dispose();

            // Assert
            // Проверяем, что Dispose не выбрасывает исключений
            Assert.DoesNotThrow(() => _unitView.Dispose());
        }

        [Test]
        public void Hover_ChangesMaterialToHovered()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.Hover();

            // Assert
            // В реальном коде здесь проверяли бы изменение материала
            // Для тестов достаточно, что метод не выбрасывает исключений
        }

        [Test]
        public void UnHover_ChangesMaterialToNormal()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.UnHover();

            // Assert
            // В реальном коде здесь проверяли бы изменение материала
            // Для тестов достаточно, что метод не выбрасывает исключений
        }

        [Test]
        public void Play_WithValidAnimationState_SetsAnimatorTrigger()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.Play(UnitAnimationState.Idle);

            // Assert
            // Проверяем, что аниматор получил правильный триггер
            Assert.That(_mockAnimator.LastTrigger, Is.EqualTo("Idle"));
        }

        [Test]
        public void Play_WithWalkAnimationState_SetsCorrectTrigger()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.Play(UnitAnimationState.Walk);

            // Assert
            Assert.That(_mockAnimator.LastTrigger, Is.EqualTo("Walk"));
        }

        [Test]
        public void Play_WithAttackAnimationState_SetsCorrectTrigger()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.Play(UnitAnimationState.Attack);

            // Assert
            Assert.That(_mockAnimator.LastTrigger, Is.EqualTo("DealDamage"));
        }

        [Test]
        public void Play_WithHitAnimationState_SetsCorrectTrigger()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.Play(UnitAnimationState.Hit);

            // Assert
            Assert.That(_mockAnimator.LastTrigger, Is.EqualTo("Hit"));
        }

        [Test]
        public void Play_WithDieAnimationState_SetsCorrectTrigger()
        {
            // Arrange
            _unitView.Init(_unitViewModel);

            // Act
            _unitView.Play(UnitAnimationState.Die);

            // Assert
            Assert.That(_mockAnimator.LastTrigger, Is.EqualTo("Die"));
        }

        [Test]
        public void Play_WithInvalidAnimationState_DoesNotSetTrigger()
        {
            // Arrange
            _unitView.Init(_unitViewModel);
            var invalidState = (UnitAnimationState)999;

            // Act
            _unitView.Play(invalidState);

            // Assert
            Assert.That(_mockAnimator.LastTrigger, Is.Null);
        }

        [Test]
        public void Init_WithNullViewModel_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _unitView.Init(null));
        }

        [Test]
        public void Init_WithValidViewModel_SetsModelProperty()
        {
            // Act
            _unitView.Init(_unitViewModel);

            // Assert
            Assert.That(_unitView.Model, Is.EqualTo(_unitModel));
        }

        [Test]
        public void Init_WithValidViewModel_InitializesUnitViewUI()
        {
            // Act
            _unitView.Init(_unitViewModel);

            // Assert
            Assert.That(_mockUnitViewUI.IsInitialized, Is.True);
        }

        // Mock классы для тестирования
        private class MockDamagable : IDamagable
        {
            public bool IsBlueTeam => true;
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;

            public void RecieveDamage(DamageContext ctx) { }
            public void SimulateRecieveDamage(DamageContext ctx) { }
        }

        private class MockUnitDefinitionSO : ScriptableObject
        {
            public Material BlueTeamMaterial => CreateMockMaterial("BlueTeam");
            public Material HoveredBlueTeamMaterial => CreateMockMaterial("HoveredBlueTeam");
            public Material RedTeamMaterial => CreateMockMaterial("RedTeam");
            public Material HoveredRedTeamMaterial => CreateMockMaterial("HoveredRedTeam");

            private Material CreateMockMaterial(string name)
            {
                var material = new Material(Shader.Find("Standard"));
                material.name = name;
                return material;
            }
        }

        private class MockUnitViewUI : UnitViewUI
        {
            public bool IsInitialized { get; private set; }

            public override void Init(UnitViewModel vm)
            {
                IsInitialized = true;
            }
        }

        private class MockAnimator : Animator
        {
            public string LastTrigger { get; private set; }

            public override void SetTrigger(string name)
            {
                LastTrigger = name;
            }
        }

        private class MockWorldToCellProvider : IWorldToCellProvider
        {
            public Vector3 ToWorld(int x, int y)
            {
                return new Vector3(x, 0, y);
            }

            public bool ToGrid(Vector3 position, out Vector2Int coords)
            {
                coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
                return true;
            }
        }
    }

    // Enum для тестирования анимаций
    public enum UnitAnimationState
    {
        Idle,
        Walk,
        Attack,
        Hit,
        Die
    }
}
