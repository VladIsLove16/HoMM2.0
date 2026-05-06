using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.Views
{
    [TestFixture]
    public class GameView3D_EditModeTests
    {
        [Test]
        public void HandleHover_InvokesHover_OnGameViewObject()
        {
            var hoverableGO = new GameObject("Hoverable");
            var stub = hoverableGO.AddComponent<HoverableStub>();
            var gameViewGO = new GameObject("GameView3D");

            try
            {
                var gameView = gameViewGO.AddComponent<GameView3D>();
                var providerField = typeof(GameView3D).GetField("_worldToCellProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                providerField?.SetValue(gameView, new WorldToCellStub());

                gameView.HandleGameViewObjectHovered(stub);

                Assert.That(stub.Hovered, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(gameViewGO);
                Object.DestroyImmediate(hoverableGO);
            }
        }

        [Test]
        public void HandleHoverWithWorldPoint_UsesProvidedHitPositionInsteadOfObjectTransform()
        {
            var hoverableGO = new GameObject("Hoverable");
            var stub = hoverableGO.AddComponent<HoverableStub>();
            var gameViewGO = new GameObject("GameView3D");

            try
            {
                var gameView = gameViewGO.AddComponent<GameView3D>();
                var providerField = typeof(GameView3D).GetField("_worldToCellProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                providerField?.SetValue(gameView, new WorldToCellByPositionStub());

                KeyValuePair<Vector2Int, Vector2Int>? received = null;
                gameView.TestHandleCellHovered = pair => received = pair;

                stub.transform.position = Vector3.zero;
                gameView.HandleGameViewObjectHovered(stub, new Vector3(12f, 0f, 0f));

                Assert.That(received.HasValue, Is.True);
                Assert.That(received.Value.Key, Is.EqualTo(new Vector2Int(5, 5)));
                Assert.That(received.Value.Value, Is.EqualTo(new Vector2Int(6, 5)));
            }
            finally
            {
                Object.DestroyImmediate(gameViewGO);
                Object.DestroyImmediate(hoverableGO);
            }
        }

        private class HoverableStub : MonoBehaviour, IGameViewObject, IHoverable
        {
            public bool Hovered { get; private set; }
            public bool IsHoverable => true;
            public bool IsSelectable => true;
            public void Hover() => Hovered = true;
            public void Unhover() => Hovered = false;
        }

        private class WorldToCellStub : IWorldToCellProvider
        {
            public Vector3 ToWorld(int x, int y) => new Vector3(x, 0f, y);
            public bool ToGrid(Vector3 position, out Vector2Int coords)
            {
                coords = Vector2Int.zero;
                return true;
            }

            public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
            {
                coords = new KeyValuePair<Vector2Int, Vector2Int>(Vector2Int.zero, Vector2Int.zero);
                return true;
            }
        }

        private class WorldToCellByPositionStub : IWorldToCellProvider
        {
            public Vector3 ToWorld(int x, int y) => new Vector3(x, 0f, y);

            public bool ToGrid(Vector3 position, out Vector2Int coords)
            {
                coords = position.x > 10f ? new Vector2Int(5, 5) : Vector2Int.zero;
                return true;
            }

            public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
            {
                coords = position.x > 10f
                    ? new KeyValuePair<Vector2Int, Vector2Int>(new Vector2Int(5, 5), new Vector2Int(6, 5))
                    : new KeyValuePair<Vector2Int, Vector2Int>(Vector2Int.zero, Vector2Int.zero);
                return true;
            }
        }
    }
}
