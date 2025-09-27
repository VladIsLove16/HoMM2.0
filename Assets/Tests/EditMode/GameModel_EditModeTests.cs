using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Model
{
	[TestFixture]
	public class GameModel_EditModeTests
	{
		private MovementSystem _movement;
		private UnitModelFactory _factory;
		private GameModel _model;

		[SetUp]
		public void SetUp()
		{
			_movement = new MovementSystem();
			var dict = TestDataFactory.CreateSingleUnitData(UnitType.Archer);
			_factory = new UnitModelFactory(dict);
			_model = new GameModel(_factory, _movement);
			_model.InitializeGrid(4, 3);
		}

		[Test]
		public void InitializeGrid_RaisesEvent_And_CreatesCells()
		{
			var cells = _model.GetAllCells();
			Assert.That(cells.GetLength(0) > 0 && cells.GetLength(1) > 0, Is.True);
		}

		[Test]
		public void SpawnUnit_AddsUnitToCell()
		{
			var stats = ScriptableObject.CreateInstance<UnitStats>();
			stats.InvulnerableEffects = new List<StatusEffectType>();
			var unit = new UnitModel(stats, UnitType.Archer, 1, 1, 1, true);

			// emulate _factory behavior
			var cell = (GameCell)_model.GetAllCells()[1,1];
			cell.AddContent(unit);

			Assert.That(cell.Unit, Is.EqualTo(unit));
			UnityEngine.Object.DestroyImmediate(stats);
		}

		[Test]
		public void MoveObject_MovesUnitBetweenCells()
		{
			var stats = ScriptableObject.CreateInstance<UnitStats>();
			stats.MoveSpeed = 10;
			stats.InvulnerableEffects = new List<StatusEffectType>();
			var unit = new UnitModel(stats, UnitType.Archer, 0, 0, 1, true);

			var start = (GameCell)_model.GetAllCells()[0,0];
			start.AddContent(unit);

			var route = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) };
			_model.MoveObject(unit, route);

			var end = (GameCell)_model.GetAllCells()[2,0];
			Assert.That(end.Unit, Is.EqualTo(unit));
			UnityEngine.Object.DestroyImmediate(stats);
		}
	}
}


