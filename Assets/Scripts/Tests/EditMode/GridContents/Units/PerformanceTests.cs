using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UniRx;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class PerformanceTests
    {
        private UnitStats _baseStats;
        private UnitModel _unitModel;
        private const int PerformanceTestIterations = 10000;

        [SetUp]
        public void SetUp()
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
        }

        [TearDown]
        public void TearDown()
        {
            if (_baseStats != null)
            {
                UnityEngine.Object.DestroyImmediate(_baseStats);
            }
        }

        [Test]
        public void UnitModel_Constructor_Performance_IsAcceptable()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var units = new List<UnitModel>();

            // Act
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                var unit = new UnitModel(_baseStats, UnitType.Archer, i, i, 10, true);
                units.Add(unit);
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)PerformanceTestIterations;
            Assert.That(averageTime, Is.LessThan(1.0), 
                $"Создание UnitModel занимает слишком много времени: {averageTime:F3}ms в среднем");

            // Cleanup
            foreach (var unit in units)
            {
                UnityEngine.Object.DestroyImmediate(unit.BaseUnitStats);
            }
        }

        [Test]
        public void UnitModel_PositionChange_Performance_IsAcceptable()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var newPosition = new Vector2Int(100, 100);

            // Act
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                _unitModel.Position.Value = new Vector2Int(i, i);
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)PerformanceTestIterations;
            Assert.That(averageTime, Is.LessThan(0.1), 
                $"Изменение позиции занимает слишком много времени: {averageTime:F3}ms в среднем");
        }

        [Test]
        public void UnitModel_DamageCalculation_Performance_IsAcceptable()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);

            // Act
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                var damageContext = _unitModel.SendDamage(attackContext);
                // Используем результат, чтобы избежать оптимизации компилятором
                if (damageContext.DamageAmount < 0) throw new Exception("Impossible");
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)PerformanceTestIterations;
            Assert.That(averageTime, Is.LessThan(0.1), 
                $"Расчет урона занимает слишком много времени: {averageTime:F3}ms в среднем");
        }

        [Test]
        public void UnitModel_StatusEffectApplication_Performance_IsAcceptable()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var statusEffect = new MockStatusEffect(StatusEffectType.Poison);

            // Act
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                _unitModel.ApplyStatusEffect(statusEffect);
                _unitModel.RemoveEffect(statusEffect);
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)PerformanceTestIterations;
            Assert.That(averageTime, Is.LessThan(0.1), 
                $"Применение/удаление эффектов занимает слишком много времени: {averageTime:F3}ms в среднем");
        }

        [Test]
        public void UnitModel_Movement_Performance_IsAcceptable()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var route = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(2, 2), new Vector2Int(3, 3) };

            // Act
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                _unitModel.MoveByRoute(route);
            }
            stopwatch.Stop();

            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)PerformanceTestIterations;
            Assert.That(averageTime, Is.LessThan(0.1), 
                $"Движение занимает слишком много времени: {averageTime:F3}ms в среднем");
        }

        [Test]
        public void UnitModel_EventSubscription_Performance_IsAcceptable()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var eventHandlers = new List<Action>();

            // Act
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                var handler = new Action(() => { });
                _unitModel.Attacked += handler;
                eventHandlers.Add(handler);
            }
            stopwatch.Stop();

            var subscribeTime = stopwatch.ElapsedMilliseconds;

            // Тестируем производительность вызова событий
            stopwatch.Reset();
            var mockTarget = new MockDamagable();
            var attackContext = new AttackContext(mockTarget);

            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                _unitModel.SendDamage(attackContext);
            }
            stopwatch.Stop();

            var eventCallTime = stopwatch.ElapsedMilliseconds;

            // Assert
            var averageSubscribeTime = subscribeTime / (double)PerformanceTestIterations;
            var averageEventCallTime = eventCallTime / 1000.0;

            Assert.That(averageSubscribeTime, Is.LessThan(0.1), 
                $"Подписка на события занимает слишком много времени: {averageSubscribeTime:F3}ms в среднем");
            Assert.That(averageEventCallTime, Is.LessThan(1.0), 
                $"Вызов событий занимает слишком много времени: {averageEventCallTime:F3}ms в среднем");

            // Cleanup
            foreach (var handler in eventHandlers)
            {
                _unitModel.Attacked -= handler;
            }
        }

        [Test]
        public void UnitModel_MemoryAllocation_Performance_IsAcceptable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(false);
            var units = new List<UnitModel>();

            // Act
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                var unit = new UnitModel(_baseStats, UnitType.Archer, i, i, 10, true);
                units.Add(unit);
            }

            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert
            var averageMemoryPerUnit = memoryIncrease / (double)PerformanceTestIterations;
            Assert.That(averageMemoryPerUnit, Is.LessThan(1024), 
                $"UnitModel потребляет слишком много памяти: {averageMemoryPerUnit:F0} байт в среднем");

            // Cleanup
            foreach (var unit in units)
            {
                UnityEngine.Object.DestroyImmediate(unit.BaseUnitStats);
            }
            units.Clear();
            GC.Collect();
        }

        [Test]
        public void UnitModel_ReactiveProperty_Performance_IsAcceptable()
        {
            // Arrange
            var stopwatch = new Stopwatch();
            var subscribers = new List<IDisposable>();

            // Act
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                var subscription = _unitModel.Position.Subscribe(pos => { });
                subscribers.Add(subscription);
            }
            stopwatch.Stop();

            var subscribeTime = stopwatch.ElapsedMilliseconds;

            // Тестируем производительность изменения значений
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < PerformanceTestIterations; i++)
            {
                _unitModel.Position.Value = new Vector2Int(i, i);
            }
            stopwatch.Stop();

            var changeTime = stopwatch.ElapsedMilliseconds;

            // Assert
            var averageSubscribeTime = subscribeTime / (double)PerformanceTestIterations;
            var averageChangeTime = changeTime / (double)PerformanceTestIterations;

            Assert.That(averageSubscribeTime, Is.LessThan(0.1), 
                $"Подписка на ReactiveProperty занимает слишком много времени: {averageSubscribeTime:F3}ms в среднем");
            Assert.That(averageChangeTime, Is.LessThan(0.1), 
                $"Изменение ReactiveProperty занимает слишком много времени: {averageChangeTime:F3}ms в среднем");

            // Cleanup
            foreach (var subscription in subscribers)
            {
                subscription.Dispose();
            }
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

        private class MockStatusEffect : StatusEffect
        {
            public MockStatusEffect(StatusEffectType type) : base(null, null, null) { }
        }
    }
}

