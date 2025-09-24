using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Controllers
{
	public class GameController_PlayModeTests
	{
		[UnityTest]
		public IEnumerator Initialize_DoesNotThrow()
		{
			var go = new GameObject("GameController");
			var ctrl = go.AddComponent<GameController>();
			ctrl.Initialize();
			yield return null;
			Assert.Pass();
		}

		[UnityTest]
		public IEnumerator Setup_SetsGridSize()
		{
			var go = new GameObject("GameController");
			var ctrl = go.AddComponent<GameController>();
			var model = new GameModel(new UnitModelFactory(), new MovementSystem());
			var turn = new TurnSystem();
			ctrl.Construct(model, turn, null, new StartupFlowStub(), new LocalUnitSpawner(model), new LocalBattleRunner(turn));
			ctrl.Setup(3, 2);
			yield return null;
			Assert.That(model.GetAllCells().GetLength(0), Is.GreaterThan(0));
		}

		private class StartupFlowStub : IGameStartupFlow
		{
			public void Run() { }
		}
	}
}


