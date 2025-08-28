using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UniRx;

namespace Tests.PlayMode.GridContents.Units
{
    public class IntegrationTests
    {
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
        private UnitView3D _unitView;
        private GameObject _gameObject;
        private UnitStats _baseStats;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Создаем базовые характеристики для тестов
            _baseStats = ScriptableObject.CreateInstance<UnitStats>();
            _baseStats.Health = 100;
            _baseStats.MaxHealth = 100;
            _baseStats.Damage = 25;
            _baseStats.SpellPower = 15;
            _baseStats.Offense = 20;
            _baseStats.Defense = 15;
            _baseStats.MoveSpeed = 3;
            _baseStats.AttackRange = 2;
            _baseStats.CanFly = false;
            _baseStats.InvulnerableEffects = new List<StatusEffectType>();

            // Создаем модель юнита
            _unitModel = new UnitModel(_baseStats, UnitType.Archer, 5, 3, 10, true);

            // Создаем мок материалов
            var materialsInfo = new UnitViewModelMaterialsInfo(new MockUnitDefinitionSO());
            
            // Создаем ViewModel
            _unitViewModel = new UnitViewModel(_unitModel, materialsInfo);

            // Создаем GameObject с компонентами
            _gameObject = new GameObject("TestUnitView");
            _gameObject.AddComponent<Animator>();
            _unitView = _gameObject.AddComponent<UnitView3D>();

            yield return null; // Ждем один кадр для инициализации
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_baseStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_baseStats);
            }
            
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitModel_UnitViewModel_UnitView3D_Integration_WorksCorrectly()
        {
            // Arrange
            var damageReceived = false;
            var attackPerformed = false;
            var turnStarted = false;

            _unitViewModel.OnHit.Subscribe(_ => damageReceived = true);
            _unitViewModel.OnAttacked.Subscribe(_ => attackPerformed = true);
            _unitViewModel.OnTurnStarted.Subscribe(_ => turnStarted = true);

            // Act - Инициализируем View
            _unitView.Init(_unitViewModel);

            // Assert - Проверяем, что View получил модель
            Assert.That(_unitView.Model, Is.EqualTo(_unitModel));

            // Act - Выполняем атаку
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);
            _unitModel.SendDamage(attackContext);

            yield return new WaitForSeconds(0.1f); // Ждем обработки событий

            // Assert - Проверяем, что события сработали
            Assert.That(attackPerformed, Is.True);

            // Act - Получаем урон
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f); // Ждем обработки событий

            // Assert - Проверяем, что события сработали
            Assert.That(damageReceived, Is.True);

            // Act - Начинаем ход
            _unitModel.TakeTurn();

            yield return new WaitForSeconds(0.1f); // Ждем обработки событий

            // Assert - Проверяем, что события сработали
            Assert.That(turnStarted, Is.True);
        }

        [UnityTest]
        public IEnumerator UnitModel_StatusEffects_Integration_WorksCorrectly()
        {
            // Arrange
            var statusEffect = new MockStatusEffect(StatusEffectType.Poison);
            var initialEffectCount = _unitModel.ActiveEffects.Count;

            // Act - Применяем эффект
            _unitModel.ApplyStatusEffect(statusEffect);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что эффект применен
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(initialEffectCount + 1));

            // Act - Удаляем эффект
            _unitModel.RemoveEffect(statusEffect);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что эффект удален
            Assert.That(_unitModel.ActiveEffects.Count, Is.EqualTo(initialEffectCount));
        }

        [UnityTest]
        public IEnumerator UnitModel_Movement_Integration_WorksCorrectly()
        {
            // Arrange
            var initialPosition = _unitModel.Position.Value;
            var route = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(2, 2), new Vector2Int(3, 3) };
            var movedEventInvoked = false;

            _unitModel.Moved += (movedRoute) => 
            {
                movedEventInvoked = true;
                Assert.That(movedRoute, Is.EqualTo(route));
            };

            // Act - Двигаем юнита
            _unitModel.MoveByRoute(route);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что позиция изменилась
            Assert.That(_unitModel.Position.Value, Is.EqualTo(new Vector2Int(3, 3)));
            Assert.That(movedEventInvoked, Is.True);
        }

        [UnityTest]
        public IEnumerator UnitModel_Combat_Integration_WorksCorrectly()
        {
            // Arrange
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);
            var initialHealth = _unitModel.ModifiedStats.Health;

            // Act - Атакуем цель
            var damageContext = _unitModel.SendDamage(attackContext);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что урон рассчитан правильно
            Assert.That(damageContext.DamageAmount, Is.EqualTo(_baseStats.Damage * _unitModel.Amount.Value));
            Assert.That(damageContext.Type, Is.EqualTo(DamageType.physical));
            Assert.That(damageContext.Source, Is.EqualTo(_unitModel));

            // Act - Симулируем атаку
            var simulatedDamageContext = _unitModel.SimulateSendDamage(attackContext);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что симуляция работает
            Assert.That(simulatedDamageContext.DamageAmount, Is.EqualTo(_baseStats.Damage * _unitModel.Amount.Value));
        }

        [UnityTest]
        public IEnumerator UnitModel_DamageCalculation_Integration_WorksCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(150, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;
            var initialHealth = _unitModel.ModifiedStats.Health;

            // Act - Получаем урон больше здоровья одного юнита
            _unitModel.RecieveDamage(damageContext);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что урон правильно распределен
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1));
            Assert.That(damageContext.DieAmount, Is.EqualTo(1));
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(initialHealth - 50));
        }

        [UnityTest]
        public IEnumerator UnitModel_Death_Integration_WorksCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            var diedEventInvoked = false;

            _unitModel.Died += () => diedEventInvoked = true;

            // Act - Получаем смертельный урон
            _unitModel.RecieveDamage(damageContext);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что юнит умер
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0));
            Assert.That(diedEventInvoked, Is.True);
        }

        [UnityTest]
        public IEnumerator UnitViewModel_MaterialSelection_Integration_WorksCorrectly()
        {
            // Arrange
            var blueTeamStats = ScriptableObject.CreateInstance<UnitStats>();
            blueTeamStats.InvulnerableEffects = new List<StatusEffectType>();
            var blueTeamModel = new UnitModel(blueTeamStats, UnitType.Archer, 0, 0, 1, true);

            var redTeamStats = ScriptableObject.CreateInstance<UnitStats>();
            redTeamStats.InvulnerableEffects = new List<StatusEffectType>();
            var redTeamModel = new UnitModel(redTeamStats, UnitType.Archer, 0, 0, 1, false);

            var materialsInfo = new UnitViewModelMaterialsInfo(new MockUnitDefinitionSO());

            // Act - Создаем ViewModels для разных команд
            var blueTeamViewModel = new UnitViewModel(blueTeamModel, materialsInfo);
            var redTeamViewModel = new UnitViewModel(redTeamModel, materialsInfo);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что материалы выбраны правильно
            Assert.That(blueTeamViewModel.TeamMaterial, Is.EqualTo(materialsInfo.BlueTeamMaterial));
            Assert.That(blueTeamViewModel.HoveredTeamMaterial, Is.EqualTo(materialsInfo.HoveredBlueTeamMaterial));
            Assert.That(redTeamViewModel.TeamMaterial, Is.EqualTo(materialsInfo.RedTeamMaterial));
            Assert.That(redTeamViewModel.HoveredTeamMaterial, Is.EqualTo(materialsInfo.HoveredRedTeamMaterial));

            // Cleanup
            UnityEngine.Object.DestroyImmediate(blueTeamStats);
            UnityEngine.Object.DestroyImmediate(redTeamStats);
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

        private class MockStatusEffect : StatusEffect
        {
            public MockStatusEffect(StatusEffectType type) : base(null, null, null) { }
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
}
