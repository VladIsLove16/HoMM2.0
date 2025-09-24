using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using UniRx;
using NUnit.Compatibility;

namespace Tests.PlayMode.GridContents.Units
{
    public class IntegrationTests
    {
        private UnitModel _unitModel;
        private UnitViewModel _unitViewModel;
        private UnitView3D _unitView;
        private GameObject _gameObject;
        private UnitStats _baseStats;
        private MaterialProvider _materialProvider;

        private int baseX = 5;

        private int baseY = 3;
        private int baseAmount = 3;

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
            _unitModel = new UnitModel(_baseStats, UnitType.Archer, baseX, baseY, baseAmount, true);

            
            // Создаем ViewModel
            _unitViewModel = new UnitViewModel(_unitModel, _materialProvider);

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
            var statusEffect = new MockStatusEffect(StatusEffectType.Armored);
            var initialEffectCount = _unitModel.ActiveEffects.Count;

            // Act - Применяем эффект
            _unitModel.ApplyEffect(statusEffect);

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

            _unitModel.MovedByRoute += (movedRoute) => 
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


            // Act - Создаем ViewModels для разных команд
            var blueTeamViewModel = new UnitViewModel(blueTeamModel, _materialProvider);
            var redTeamViewModel = new UnitViewModel(redTeamModel, _materialProvider);

            yield return null; // Ждем один кадр

            // Assert - Проверяем, что материалы выбраны правильно
            Assert.That(blueTeamViewModel.TeamMaterial, Is.EqualTo(_materialProvider.BlueTeamMaterial));
            Assert.That(blueTeamViewModel.HoveredTeamMaterial, Is.EqualTo(_materialProvider.HoveredBlueTeamMaterial));
            Assert.That(redTeamViewModel.TeamMaterial, Is.EqualTo(_materialProvider.RedTeamMaterial));
            Assert.That(redTeamViewModel.HoveredTeamMaterial, Is.EqualTo(_materialProvider.HoveredRedTeamMaterial));

            // Cleanup
            UnityEngine.Object.DestroyImmediate(blueTeamStats);
            UnityEngine.Object.DestroyImmediate(redTeamStats);
        }

        [UnityTest]
        public IEnumerator UnitView3D_Attack_DoesNotDeactivateGameObject()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null); // Урон меньше здоровья
            var initialAmount = _unitModel.Amount.Value;
            var gameObjectActiveBefore = _unitView.gameObject.activeSelf;

            // Act - Атакуем юнита, но не убиваем его полностью
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(2f); // Ждем завершения анимации

            // Assert - GameObject должен остаться активным
            Assert.That(_unitView.gameObject.activeSelf, Is.True, 
                "GameObject should remain active after attack that doesn't kill the unit");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount), 
                "Unit amount should remain the same after non-lethal damage");
        }

        [UnityTest]
        public IEnumerator UnitView3D_Death_DeactivatesGameObject()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null); // Смертельный урон
            var gameObjectActiveBefore = _unitView.gameObject.activeSelf;

            // Act - Убиваем юнита полностью
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(2f); // Ждем завершения анимации смерти

            // Assert - GameObject должен быть деактивирован
            Assert.That(_unitView.gameObject.activeSelf, Is.False, 
                "GameObject should be deactivated after unit death");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0), 
                "Unit amount should be 0 after lethal damage");
        }

        [UnityTest]
        public IEnumerator UnitView3D_PartialDamage_HandlesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(150, DamageType.physical, null); // Урон больше здоровья одного юнита
            var initialAmount = _unitModel.Amount.Value;

            // Act - Атакуем юнита, убиваем одного, но не весь стэк
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(2f);

            // Assert - GameObject должен остаться активным, количество уменьшилось
            Assert.That(_unitView.gameObject.activeSelf, Is.True, 
                "GameObject should remain active after partial stack death");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_unitModel.BaseUnitStats.MaxHealth), 
                "Remaining unit should have full health");
        }

        [UnityTest]
        public IEnumerator UnitView3D_Events_TriggerCorrectly()
        {
            // Arrange
            var onDeathInvoked = false;
            var onHitInvoked = false;
            
            _unitViewModel.OnDeath.Subscribe(_ => onDeathInvoked = true);
            _unitViewModel.OnHit.Subscribe(_ => onHitInvoked = true);

            // Act - Атакуем юнита, но не убиваем
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - OnHit должен вызваться, OnDeath - нет
            Assert.That(onHitInvoked, Is.True, "OnHit should be invoked");
            Assert.That(onDeathInvoked, Is.False, "OnDeath should not be invoked for non-lethal damage");
        }

        [UnityTest]
        public IEnumerator UnitView3D_Animations_PlayCorrectly()
        {
            // Arrange
            var animator = _unitView.GetComponent<Animator>();
            var initialAnimatorState = animator.GetCurrentAnimatorStateInfo(0);

            // Act - Атакуем юнита
            var damageContext = new DamageContext(50, DamageType.physical, null);
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Анимация Hit должна играться
            var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
            Assert.That(currentAnimatorState.IsName("Hit") || currentAnimatorState.IsName("Base Layer"), 
                "Hit animation should play or return to base state");
        }

        [UnityTest]
        public IEnumerator UnitView3D_DeathAnimation_PlaysBeforeDeactivation()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null); // Смертельный урон
            var animator = _unitView.GetComponent<Animator>();

            // Act - Убиваем юнита полностью
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.5f); // Ждем начала анимации смерти

            // Assert - GameObject еще активен, анимация смерти играется
            Assert.That(_unitView.gameObject.activeSelf, Is.True, 
                "GameObject should still be active during death animation");
            
            var currentAnimatorState = animator.GetCurrentAnimatorStateInfo(0);
            Assert.That(currentAnimatorState.IsName("Die"), 
                "Death animation should be playing");

            yield return new WaitForSeconds(2f); // Ждем завершения анимации

            // Assert - Теперь GameObject деактивирован
            Assert.That(_unitView.gameObject.activeSelf, Is.False, 
                "GameObject should be deactivated after death animation");
        }

        [UnityTest]
        public IEnumerator UnitViewUI_HealthRatio_UpdatesCorrectlyAfterDamage()
        {
            // Arrange
            var damageContext = new DamageContext(50, DamageType.physical, null);
            var initialHealth = _unitModel.ModifiedStats.Health;
            var maxHealth = _unitModel.ModifiedStats.MaxHealth;
            var expectedRatio = (float)(initialHealth - 50) / maxHealth;

            // Act - Наносим урон
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Ratio должен обновиться
            var actualRatio = _unitViewModel.HealthRatio;
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be updated correctly after damage");
            Assert.That(actualRatio, Is.EqualTo(0.5f), 
                "Health ratio should be 0.5 (50%) after 50 damage to 100 health");
        }

        [UnityTest]
        public IEnumerator UnitViewUI_UnitAmount_UpdatesCorrectlyAfterDamage()
        {
            // Arrange
            var damageContext = new DamageContext(100, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act - Убиваем одного юнита
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Количество юнитов должно уменьшиться
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1 after lethal damage");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_unitModel.BaseUnitStats.MaxHealth), 
                "Remaining unit should have full health");
        }

        [UnityTest]
        public IEnumerator UnitViewUI_HealthRatio_WithMultipleUnitDeaths_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(250, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act - Убиваем 2 юнита (100 + 100 = 200) и повреждаем третьего на 50
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Количество и здоровье должны обновиться
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 2), 
                "Unit amount should decrease by 2 after killing 2 units");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(50), 
                "Remaining unit should have 50 health (100 - 50)");
            
            var actualRatio = _unitViewModel.HealthRatio;
            var expectedRatio = 0.5f; // 50/100
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be 0.5 (50%) after multiple unit deaths");
        }

        [UnityTest]
        public IEnumerator UnitViewUI_HealthRatio_WithPartialDamage_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(30, DamageType.physical, null);
            var initialHealth = _unitModel.ModifiedStats.Health;
            var maxHealth = _unitModel.ModifiedStats.MaxHealth;

            // Act - Наносим небольшой урон
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Ratio должен обновиться, количество не изменится
            var actualRatio = _unitViewModel.HealthRatio;
            var expectedRatio = (float)(initialHealth - 30) / maxHealth;
            
            Assert.That(actualRatio, Is.EqualTo(expectedRatio), 
                "Health ratio should be updated correctly after partial damage");
            Assert.That(actualRatio, Is.EqualTo(0.7f), 
                "Health ratio should be 0.7 (70%) after 30 damage to 100 health");
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(baseAmount), 
                "Unit amount should remain the same after non-lethal damage");
        }

        [UnityTest]
        public IEnumerator UnitViewUI_HealthRatio_WithExactKill_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(_unitModel.ModifiedStats.MaxHealth , DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act - Убиваем точно одного юнита
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Количество уменьшится на 1, здоровье восстановится
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(initialAmount - 1), 
                "Unit amount should decrease by 1 after exact kill");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(_unitModel.BaseUnitStats.MaxHealth), 
                "Remaining unit should have full health");
            
            var actualRatio = _unitViewModel.HealthRatio;
            Assert.That(actualRatio, Is.EqualTo(1.0f), 
                "Health ratio should be 1.0 (100%) after next unit gets full health");
        }

        [UnityTest]
        public IEnumerator UnitViewUI_HealthRatio_WithOverkill_UpdatesCorrectly()
        {
            // Arrange
            var damageContext = new DamageContext(1000, DamageType.physical, null);
            var initialAmount = _unitModel.Amount.Value;

            // Act - Убиваем всех юнитов
            _unitModel.RecieveDamage(damageContext);

            yield return new WaitForSeconds(0.1f);

            // Assert - Все юниты должны умереть
            Assert.That(_unitModel.Amount.Value, Is.EqualTo(0), 
                "Unit amount should be 0 after overkill damage");
            Assert.That(_unitModel.ModifiedStats.Health, Is.EqualTo(0), 
                "Unit health should be 0 after overkill damage");
            
            var actualRatio = _unitViewModel.HealthRatio;
            Assert.That(actualRatio, Is.EqualTo(0f), 
                "Health ratio should be 0 when all units are dead");
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

        private class MockUnitDefinitionSO : UnitDefinitionSO
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

            public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
            {
                var key = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
                var value = new Vector2Int(Mathf.RoundToInt(position.x+1), Mathf.RoundToInt(position.z));
                coords = new(key, value);
                return true;
            }
        }
    }
}
