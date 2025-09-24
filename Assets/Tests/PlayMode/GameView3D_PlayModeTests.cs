using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Views
{
	public class GameView3D_PlayModeTests
	{
		[UnityTest]
		public IEnumerator HandleHover_InvokesHover_OnGameViewObject()
		{
			var go = new GameObject("Hoverable");
			var stub = go.AddComponent<HoverableStub>();
			var gameView = new GameObject("GameView3D").AddComponent<GameView3D>();

			// Inject stub world-to-cell provider via reflection
			var providerField = typeof(GameView3D).GetField("_worldToCellProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			providerField?.SetValue(gameView, new WorldToCellStub());

			gameView.HandleGameViewObjectHovered(stub);
			yield return null;
			Assert.That(stub.Hovered, Is.True);
		}

		private class HoverableStub : MonoBehaviour, IGameViewObject, IHoverable
		{
			public bool Hovered { get; private set; }
			public bool IsHoverable => true;
			public bool IsSelectable => true;
			public void Hover() { Hovered = true; }
			public void Unhover() { Hovered = false; }
		}

		private class WorldToCellStub : IWorldToCellProvider
		{
			public Vector3 ToWorld(int x, int y) => new Vector3(x, 0, y);
			public bool ToGrid(Vector3 position, out Vector2Int coords) { coords = Vector2Int.zero; return true; }
			public bool ToGridPair(Vector3 position, out System.Collections.Generic.KeyValuePair<Vector2Int, Vector2Int> coords)
			{
				coords = new System.Collections.Generic.KeyValuePair<Vector2Int, Vector2Int>(Vector2Int.zero, Vector2Int.zero);
				return true;
			}
		}
	}
}

