using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
namespace Tests.EditMode.Actions
{
    [TestFixture]
    public class ActionResolver_EditModeTests
    {
        private const int x = 0;
        private const int y = 0;
         GameModel _model;
        MovementSystem _movementSystem;
        [SetUp]
        public void Setup()
        {
            UnitDefinitionSO unitDefinitionSO = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            var dict = new Dictionary<UnitType, UnitDefinitionSO>
            {
                { UnitType.Archer, unitDefinitionSO }
            };
            var factory = new UnitModelFactory(dict);
            var baseStats = ScriptableObject.CreateInstance<UnitStats>();
            unitDefinitionSO.Stats = baseStats;

            _movementSystem = new MovementSystem();

            _model = new(factory, _movementSystem);

            _model.InitializeGrid(3, 3);
            _model.SpawnUnit(new UnitSpawnParams(x, y, UnitType.Archer, 1, true));
        }
        [Test]
        public void Resolve_ReturnsMoveHandler_ForSimpleMoveContext()
        {
            var resolver = new ActionResolver(_model, _movementSystem);
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 0), SpellType.None, default);

            var result = resolver.Resolve(ctx, out var handler);

            Assert.That(result, Is.True, "Должен быть найден обработчик для простого перемещения");
            Assert.That(handler, Is.Not.Null, "Обработчик не должен быть null");
            Assert.That(handler, Is.TypeOf<MoveActionHandler>(), "Ожидается MoveActionHandler");
        }

        [Test]
        public void Resolve_ReturnsNoHandler_ForInvalidContext()
        {
            var resolver = new ActionResolver(_model, new MovementSystem());
            // Некорректный контекст: перемещение в ту же клетку
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(0, 0), default, default);

            var result = resolver.Resolve(ctx, out var handler);

            Assert.That(result, Is.False, "Не должен быть найден обработчик для некорректного действия");
            Assert.That(handler, Is.Null, "Обработчик должен быть null");
        }
    }
}
