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
		private UnitViewModelFactory _uvmFactory;
		private GameViewModel _vm;

		[SetUp]
		public void SetUp()
		{
			_model = new GameModel(new UnitModelFactory(), new MovementSystem());
			_movement = new MovementSystem();
			_turnSystem = new TurnSystem();
			_uvmFactory = new UnitViewModelFactory();
			_vm = new GameViewModel(_model, _movement, new ActionExecutorStub(), _turnSystem, _uvmFactory);
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

		private class ActionExecutorStub : IActionExecutor
		{
			public void Execute(IActionHandler actionHandler, ActionContext context) { }
		}
	}
}


