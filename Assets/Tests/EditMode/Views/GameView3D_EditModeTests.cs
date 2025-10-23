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
    }
}