using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Input
{
    // Integration tests that verify a cell rendered by PerCellGridRenderer
    // becomes hovered/selected when the grid view model publishes a preview
    // and when GameView3D -> GameViewModel path is invoked (the path used by CellInputHandler).
    [TestFixture]
    public class CellInputHandler_IntegrationTests
    {
        private GameObject _rendererGO;
        private PerCellGridRenderer _renderer;
        private GameObject _prefab;
        private GameObject _parent;
        private TestGridViewModel _gridVM;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // create a simple CellView prefab (non-asset) with required renderers
            _prefab = new GameObject("CellViewPrefab");
            var mainRenderer = _prefab.AddComponent<MeshRenderer>();
            var filter = _prefab.AddComponent<MeshFilter>();
            filter.mesh = MeshGenerator.CreateQuad();
            var cellView = _prefab.AddComponent<CellView>();

            // hover renderer child
            var hover = new GameObject("hover");
            hover.transform.SetParent(_prefab.transform);
            var hoverR = hover.AddComponent<MeshRenderer>();
            var hoverFilter = hover.AddComponent<MeshFilter>();
            hoverFilter.mesh = MeshGenerator.CreateQuad();

            // route renderer child
            var route = new GameObject("route");
            route.transform.SetParent(_prefab.transform);
            var routeR = route.AddComponent<MeshRenderer>();
            var routeFilter = route.AddComponent<MeshFilter>();
            routeFilter.mesh = MeshGenerator.CreateQuad();

            // assign via public setters
            cellView.SetHoverRenderer(hoverR);
            cellView.SetRoutePointRenderer(routeR);

            // create renderer
            _rendererGO = new GameObject("PerCellGridRenderer");
            _renderer = _rendererGO.AddComponent<PerCellGridRenderer>();

            // parent for instantiated cells
            _parent = new GameObject("cells_parent");

            // set prefab and parent on renderer
            // assign prefab and parent through public setters
            _renderer.SetPrefab(cellView);
            _renderer.SetParent(_parent);

            // prepare materials list (normal, hovered, selected, reachable)
            Material Make(string name) { var m = new Material(Shader.Find("Standard")); m.name = name; return m; }
            var normal = new CellMaterial() { CellState = CellState.normal, Material = Make("normal") };
            var hovered = new CellMaterial() { CellState = CellState.hovered, Material = Make("hovered") };
            var selected = new CellMaterial() { CellState = CellState.selected, Material = Make("selected") };
            var reachable = new CellMaterial() { CellState = CellState.reachableCell, Material = Make("reachable") };

            var list = new List<CellMaterial>() { normal, hovered, selected, reachable };
            _renderer.SetMaterials(list);

            // ensure Awake runs after Materials have been assigned so materialsDict is initialized
            // Awake will be called when the GameObject is activated; make sure it's active
            _rendererGO.SetActive(true);

            // small render
            _renderer.Render(3, 3, 1f, Vector3.zero, 0.4f);

            // create test grid view model and bind
            _gridVM = new TestGridViewModel();
            _renderer.Bind(_gridVM);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_rendererGO != null) Object.DestroyImmediate(_rendererGO);
            if (_prefab != null) Object.DestroyImmediate(_prefab);
            if (_parent != null) Object.DestroyImmediate(_parent);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PerCellGridRenderer_HighlightsCell_OnPreviewChanged()
        {
            // pick a cell coords and find instantiated CellView
            var target = new Vector2Int(1, 1);
            var world = _renderer.ToWorld(target.x, target.y);
            // find by name which ends with "1 1"
            CellView targetCell = null;
            foreach (Transform t in _parent.transform)
            {
                if (t.gameObject.name.Contains("1 1"))
                {
                    targetCell = t.GetComponent<CellView>();
                    break;
                }
            }
            Assert.IsNotNull(targetCell, "Target cell was not created");

            // raise preview with hovered state for that cell
            var preview = new PreviewResult();
            preview.Add(CellState.hovered, new List<Vector2Int> { target });
            _gridVM.RaisePreview(preview);

            yield return null;

            // hovered renderer should be active
            var states = targetCell.GetStates();
            Assert.That(System.Array.Exists(states, s => s == CellState.hovered));
            // The test-set hover renderer component should be active when hovered
            Assert.That(targetCell.GetComponentInChildren<MeshRenderer>(true).gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator GameView3D_Path_HoverAndSelect_UpdatesRenderer()
        {
            // Arrange: find a target cell and create GameView3D + stub GameViewModel that forwards to gridVM
            var target = new Vector2Int(0, 2);
            CellView targetCell = null;
            foreach (Transform t in _parent.transform)
            {
                if (t.gameObject.name.Contains("0 2"))
                {
                    targetCell = t.GetComponent<CellView>();
                    break;
                }
            }
            Assert.IsNotNull(targetCell);

            var gameViewGO = new GameObject("GameView3D");
            var gv = gameViewGO.AddComponent<GameView3D>();

            // inject world to cell provider that maps transform.position to the desired keypair
            var provider = new WorldToCellStub((coords) => new KeyValuePair<Vector2Int, Vector2Int>(coords, coords));
            var providerField = typeof(GameView3D).GetField("_worldToCellProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            providerField.SetValue(gv, provider);

            // create a stub GameViewModel that when HandleCellHovered/Selected is called will forward Preview to gridVM
            var stubGameVM = new StubGameViewModel((pair) =>
            {
                var preview = new PreviewResult();
                preview.Add(CellState.hovered, new List<Vector2Int> { pair.Value });
                _gridVM.RaisePreview(preview);
            }, (pair) =>
            {
                var preview = new PreviewResult();
                preview.Add(CellState.selected, new List<Vector2Int> { pair.Value });
                _gridVM.RaisePreview(preview);
            });
            // inject into gv
            var gameVMField = typeof(GameView3D).GetField("_gameVM", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gameVMField.SetValue(gv, stubGameVM);

            // Act - hover
            gv.HandleGameViewObjectHovered(targetCell);
            yield return null;

            // Assert hovered
            var states = targetCell.GetStates();
            Assert.That(System.Array.Exists(states, s => s == CellState.hovered));

            // Act - select
            gv.HandleGameViewObjectSelected(targetCell);
            yield return null;

            // Assert selected
            states = targetCell.GetStates();
            Assert.That(System.Array.Exists(states, s => s == CellState.selected));
        }

        // --- helpers ---
        private class TestGridViewModel : IGridViewModel
        {
            public System.Action<GridXZ<GameCell>> GridInited { get; set; }
            public System.Action<PreviewResult> PreviewChanged { get; set; }
            public void RaisePreview(PreviewResult r) => PreviewChanged?.Invoke(r);
        }

        private class WorldToCellStub : IWorldToCellProvider
        {
            private System.Func<Vector2Int, KeyValuePair<Vector2Int, Vector2Int>> _map;
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

        private class StubGameViewModel
        {
            private System.Action<KeyValuePair<Vector2Int, Vector2Int>> _onHover;
            private System.Action<KeyValuePair<Vector2Int, Vector2Int>> _onSelect;
            public StubGameViewModel(System.Action<KeyValuePair<Vector2Int, Vector2Int>> hover, System.Action<KeyValuePair<Vector2Int, Vector2Int>> select)
            {
                _onHover = hover;
                _onSelect = select;
            }
            public void HandleCellHovered(KeyValuePair<Vector2Int, Vector2Int> pair) => _onHover(pair);
            public void HandleCellSelected(KeyValuePair<Vector2Int, Vector2Int> pair) => _onSelect(pair);
        }

        // small mesh generator for quad used by MeshFilter
        private static class MeshGenerator
        {
            public static Mesh CreateQuad()
            {
                var m = new Mesh();
                m.vertices = new Vector3[] { new Vector3(-0.5f,0, -0.5f), new Vector3(0.5f,0,-0.5f), new Vector3(0.5f,0,0.5f), new Vector3(-0.5f,0,0.5f) };
                m.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
                m.triangles = new int[] { 0,1,2, 0,2,3 };
                m.RecalculateNormals();
                return m;
            }
        }
    }
}
