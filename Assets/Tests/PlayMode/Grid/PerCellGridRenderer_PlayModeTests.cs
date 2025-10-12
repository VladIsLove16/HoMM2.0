using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.PlayMode.Grid
{
    [TestFixture]
    public class PerCellGridRenderer_PlayModeTests
    {
        private readonly List<GameObject> _objects = new();

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var go in _objects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            _objects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Render_CreatesCellsWithExpectedCount()
        {
            var fixture = CreateRenderer();
            fixture.Renderer.Render(3, 2, 1f, Vector3.zero, 0.1f);
            yield return null;

            Assert.That(fixture.Parent.transform.childCount, Is.EqualTo(6));
        }

        [UnityTest]
        public IEnumerator AddState_ActivatesHoverRenderer()
        {
            var fixture = CreateRenderer();
            fixture.Renderer.Render(2, 2, 1f, Vector3.zero, 0.1f);
            yield return null;

            fixture.Renderer.AddState(new Vector2Int(1, 1), CellState.hovered);
            yield return null;

            var cell = FindCell(fixture.Parent, 1, 1);
            var hover = cell.transform.Find("Hover");
            Assert.That(hover.gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator RemoveState_DisablesHoverRenderer()
        {
            var fixture = CreateRenderer();
            fixture.Renderer.Render(2, 2, 1f, Vector3.zero, 0.1f);
            yield return null;

            fixture.Renderer.AddState(new Vector2Int(0, 0), CellState.hovered);
            yield return null;
            fixture.Renderer.RemoveState(new Vector2Int(0, 0), CellState.hovered);
            yield return null;

            var cell = FindCell(fixture.Parent, 0, 0);
            var hover = cell.transform.Find("Hover");
            Assert.That(hover.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator Bind_PreviewChangedAppliesSelectedState()
        {
            var fixture = CreateRenderer();
            var vm = new TestGridViewModel();
            fixture.Renderer.Bind(vm);
            fixture.Renderer.Render(2, 2, 1f, Vector3.zero, 0.1f);
            yield return null;

            var preview = new PreviewResult();
            preview.Add(CellState.selected, new[] { new Vector2Int(1, 0) });
            vm.RaisePreview(preview);
            yield return null;

            var states = fixture.Renderer.GetCellStates(new Vector2Int(1, 0));
            Assert.That(states, Does.Contain(CellState.selected));
        }
        [UnityTest]
        public void PreviewChanged_HoverStateActivatesHoverRenderer()
        {
            var fixture = CreateRenderer();
            var target = new Vector2Int(1, 1);
            var hoverPreview = new PreviewResult();
            hoverPreview.Add(CellState.hovered, new List<Vector2Int> { target });
            bool previewChanged = false;
            var vm = new TestGridViewModel();
            vm.PreviewChanged += (preview) => previewChanged = true;
            fixture.Renderer.Bind(vm);
            vm.RaisePreview(hoverPreview);
            Assert.IsTrue(previewChanged);
            Assert.IsTrue(fixture.Renderer.GetCellStates(target).Contains(CellState.hovered));
        }
        private CellView FindCell(GameObject parent, int x, int y)
        {
            foreach (Transform child in parent.transform)
            {
                if (child.name.EndsWith($"{x} {y}"))
                {
                    return child.GetComponent<CellView>();
                }
            }
            Assert.Fail($"Cell {x},{y} not found");
            return null;
        }

        private RendererFixture CreateRenderer()
        {
            var parent = new GameObject("CellsParent");
            _objects.Add(parent);

            var prefab = CreateCellPrefab();
            _objects.Add(prefab);

            var host = new GameObject("PerCellGridRenderer");
            var renderer = host.AddComponent<PerCellGridRenderer>();
            _objects.Add(host);

            SetPrivateField(renderer, "prefab", prefab.GetComponent<CellView>());
            SetPrivateField(renderer, "parent", parent);
            SetPrivateField(renderer, "Materials", CreateMaterials());

            return new RendererFixture(renderer, parent);
        }

        private GameObject CreateCellPrefab()
        {
            var prefab = new GameObject("CellPrefab");
            var meshFilter = prefab.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateQuad();
            prefab.AddComponent<MeshRenderer>();
            var cellView = prefab.AddComponent<CellView>();

            var hover = new GameObject("Hover");
            hover.transform.SetParent(prefab.transform);
            var hoverRenderer = hover.AddComponent<MeshRenderer>();
            hover.AddComponent<MeshFilter>().sharedMesh = CreateQuad();
            cellView.SetHoverRenderer(hoverRenderer);

            var route = new GameObject("Route");
            route.transform.SetParent(prefab.transform);
            var routeRenderer = route.AddComponent<MeshRenderer>();
            route.AddComponent<MeshFilter>().sharedMesh = CreateQuad();
            cellView.SetRoutePointRenderer(routeRenderer);

            return prefab;
        }

        private List<CellMaterial> CreateMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            return new List<CellMaterial>
            {
                new CellMaterial( CellState.normal,  new Material(shader) { name = "Normal" } ),
                new CellMaterial( CellState.hovered,new Material(shader) { name = "Hover" } ),
                new CellMaterial( CellState.selected, new Material(shader) { name = "Selected" } ),
                new CellMaterial( CellState.reachableCell,new Material(shader) { name = "Reachable" } )
            };
        }

        private Mesh CreateQuad()
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

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private sealed class RendererFixture
        {
            public RendererFixture(PerCellGridRenderer renderer, GameObject parent)
            {
                Renderer = renderer;
                Parent = parent;
            }

            public PerCellGridRenderer Renderer { get; }
            public GameObject Parent { get; }
        }

        private sealed class TestGridViewModel : IGridViewModel
        {
            public event System.Action<int, int> GridInited;
            public event System.Action<PreviewResult> PreviewChanged;

            public event System.Action<PreviewResult> PreviewUpdated;

            public void RaisePreview(PreviewResult result) => PreviewChanged?.Invoke(result);
        }
    }
}
