using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UniRx;

namespace Tests.EditMode.ViewModels
{
	[TestFixture]
	public class GameViewModel_EditModeTests
	{
		private GameModel _model;
		private MovementSystem _movement;
		private TurnSystem _turnSystem;
		private GameViewModel _vm;

		[SetUp]
		public void SetUp()
		{
			var dict = TestDataFactory.CreateSingleUnitData(UnitType.Archer);
			_movement = new MovementSystem();
			_model = new GameModel(new UnitModelFactory(dict), _movement);
			_turnSystem = new TurnSystem();
			var actionPipeline = new ActionPipeline(new(_model, _movement), _turnSystem);

            _vm = new GameViewModel(_model, _movement, new LocalGameCommandExecutor(actionPipeline), _turnSystem );
			_model.InitializeGrid(3, 3);
		}

		[Test]
		public void GridInitialized_Event_Fires()
		{
			int w = 0, h = 0;
			_vm.GridInitialized += (W, H) => { w = W; h = H; };
			_model.InitializeGrid(5, 4);
			Assert.That((w, h), Is.EqualTo((5, 4)));
		}

	}
}


