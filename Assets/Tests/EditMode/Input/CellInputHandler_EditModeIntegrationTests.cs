using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.EditMode.Input
{
    [TestFixture]
    public class CellInputHandler_EditModeIntegrationTests
    {
        private GameObject _rendererGO;
        private PerCellGridRenderer _renderer;
        private GameObject _prefab;
        private GameObject _parent;
        private TestGridViewModel _gridVM;

        [SetUp]
        public void SetUp()
        {
            _prefab = new GameObject("CellViewPrefab");
            _prefab.AddComponent<MeshRenderer>();
            var filter = _prefab.AddComponent<MeshFilter>();
            filter.mesh = MeshGenerator.CreateQuad();
            var cellView = _prefab.AddComponent<CellView>();

            var hover = new GameObject("hover");
            hover.transform.SetParent(_prefab.transform);
            var hoverRenderer = hover.AddComponent<MeshRenderer>();
            hover.AddComponent<MeshFilter>().mesh = MeshGenerator.CreateQuad();

            var route = new GameObject("route");
            route.transform.SetParent(_prefab.transform);
            var routeRenderer = route.AddComponent<MeshRenderer>();
            route.AddComponent<MeshFilter>().mesh = MeshGenerator.CreateQuad();

            cellView.SetHoverRenderer(hoverRenderer);
            cellView.SetRoutePointRenderer(routeRenderer);

            _rendererGO = new GameObject("PerCellGridRenderer");
            _renderer = _rendererGO.AddComponent<PerCellGridRenderer>();

            _parent = new GameObject("cells_parent");
            _renderer.SetPrefab(cellView);
            _renderer.SetParent(_parent);

            var materials = new List<CellMaterial>()
            {
                MakeMaterial(CellState.normal, "normal"),
                MakeMaterial(CellState.hovered, "hovered"),
                MakeMaterial(CellState.selected, "selected"),
                MakeMaterial(CellState.reachableCell, "reachable"),
            };
            _renderer.SetMaterials(materials);

            _renderer.Render(3, 3, 1f, Vector3.zero, 0.4f);

            _gridVM = new TestGridViewModel();
            _renderer.Bind(_gridVM);
        }

        [TearDown]
        public void TearDown()
        {
            if (_rendererGO != null) Object.DestroyImmediate(_rendererGO);
            if (_prefab != null) Object.DestroyImmediate(_prefab);
            if (_parent != null) Object.DestroyImmediate(_parent);
        }

        [Test]
        public void PerCellGridRenderer_HighlightsCell_OnPreviewChanged()
        {
            var target = new Vector2Int(1, 1);
            var targetCell = FindCell(target);

            var preview = new PreviewResult();
            preview.Add(CellState.hovered, new List<Vector2Int> { target });
            _gridVM.RaisePreview(preview);

            var states = targetCell.GetStates();
            Assert.That(System.Array.Exists(states, s => s == CellState.hovered));
            Assert.That(targetCell.GetComponentInChildren<MeshRenderer>(true).gameObject.activeSelf, Is.True);
        }

        [Test]
        public void GameView3D_Path_HoverAndSelect_UpdatesRenderer()
        {
            var target = new Vector2Int(0, 2);
            var targetCell = FindCell(target);

            var gameViewGO = new GameObject("GameView3D");
            try
            {
                var gameView = gameViewGO.AddComponent<GameView3D>();
                var provider = new WorldToCellStub(coords => new KeyValuePair<Vector2Int, Vector2Int>(coords, coords));
                gameView.SetWorldToCellProvider(provider);

                gameView.TestHandleCellHovered = pair =>
                {
                    var hoverPreview = new PreviewResult();
                    hoverPreview.Add(CellState.hovered, new List<Vector2Int> { pair.Value });
                    _gridVM.RaisePreview(hoverPreview);
                };

                gameView.TestHandleCellSelected = pair =>
                {
                    var selectPreview = new PreviewResult();
                    selectPreview.Add(CellState.selected, new List<Vector2Int> { pair.Value });
                    _gridVM.RaisePreview(selectPreview);
                };

                gameView.HandleGameViewObjectHovered(targetCell);
                var statesAfterHover = targetCell.GetStates();
                Assert.That(System.Array.Exists(statesAfterHover, s => s == CellState.hovered));

                gameView.HandleGameViewObjectSelected(targetCell);
                var statesAfterSelect = targetCell.GetStates();
                Assert.That(System.Array.Exists(statesAfterSelect, s => s == CellState.selected));
            }
            finally
            {
                Object.DestroyImmediate(gameViewGO);
            }
        }

        private CellView FindCell(Vector2Int coords)
        {
            foreach (Transform child in _parent.transform)
            {
                if (child.gameObject.name.Contains($"{coords.x} {coords.y}"))
                {
                    return child.GetComponent<CellView>();
                }
            }

            Assert.Fail($"Cell at {coords} was not created");
            return null;
        }

        private CellMaterial MakeMaterial(CellState state, string name)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { name = name };
            return new CellMaterial(state,material);
        }

        private class TestGridViewModel : IGridViewModel
        {
            public event System.Action<int,int> GridInited;
            public event System.Action<PreviewResult> PreviewChanged;
            public event System.Action<PreviewResult> PreviewUpdated;

            public void RaisePreview(PreviewResult result) => PreviewChanged?.Invoke(result);
        }

        private class WorldToCellStub : IWorldToCellProvider
        {
            private readonly System.Func<Vector2Int, KeyValuePair<Vector2Int, Vector2Int>> _map;

            public WorldToCellStub(System.Func<Vector2Int, KeyValuePair<Vector2Int, Vector2Int>> map)
            {
                _map = map;
            }

            public Vector3 ToWorld(int x, int y) => new Vector3(x, 0, y);

            public bool ToGrid(Vector3 position, out Vector2Int coords)
            {
                coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
                return true;
            }

            public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coordPair)
            {
                var coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
                coordPair = _map(coords);
                return true;
            }
        }

        private static class MeshGenerator
        {
            public static Mesh CreateQuad()
            {
                var mesh = new Mesh();
                mesh.vertices = new[]
                {
                    new Vector3(-0.5f, 0f, -0.5f),
                    new Vector3(0.5f, 0f, -0.5f),
                    new Vector3(0.5f, 0f, 0.5f),
                    new Vector3(-0.5f, 0f, 0.5f)
                };
                mesh.uv = new[]
                {
                    Vector2.zero,
                    Vector2.right,
                    Vector2.one,
                    Vector2.up
                };
                mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                mesh.RecalculateNormals();
                return mesh;
            }
        }
    }
}
