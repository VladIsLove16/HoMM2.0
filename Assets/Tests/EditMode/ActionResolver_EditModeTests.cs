using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
namespace Tests.EditMode.Actions
{
    [TestFixture]
    public class ActionResolver_EditModeTests
    {
        private GameModel CreateModel()
        {
            var factory = new UnitModelFactory();
            var unitSO = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            var baseStats = ScriptableObject.CreateInstance<UnitStats>();
            unitSO.Stats = baseStats;
            factory.Add(UnitType.Witch, unitSO);

            var model = new GameModel(new UnitModelFactory(), new MovementSystem());
            model.InitializeGrid(3, 3);
            model.SpawnUnit(new UnitSpawnParams(0,0,UnitType.Witch,1,true));
            return model;
        }

        private ActionContext CreateMoveContext()
        {
            return new ActionContext(new Vector2Int(0, 0), new Vector2Int(1, 0), default,default);
        }

        [Test]
        public void Resolve_ReturnsMoveHandler_ForSimpleMoveContext()
        {
            var model = CreateModel();
            var resolver = new ActionResolver(model, new MovementSystem());
            var ctx = CreateMoveContext();

            var result = resolver.Resolve(ctx, out var handler);

            Assert.That(result, Is.True, "Должен быть найден обработчик для простого перемещения");
            Assert.That(handler, Is.Not.Null, "Обработчик не должен быть null");
            Assert.That(handler, Is.TypeOf<MoveActionHandler>(), "Ожидается MoveActionHandler");
        }

        [Test]
        public void Resolve_ReturnsNoHandler_ForInvalidContext()
        {
            var model = CreateModel();
            var resolver = new ActionResolver(model, new MovementSystem());
            // Некорректный контекст: перемещение в ту же клетку
            var ctx = new ActionContext(new Vector2Int(0, 0), new Vector2Int(0, 0), default, new Vector2Int(0, 0));

            var result = resolver.Resolve(ctx, out var handler);

            Assert.That(result, Is.False, "Не должен быть найден обработчик для некорректного действия");
            Assert.That(handler, Is.Null, "Обработчик должен быть null");
        }
    }
}


