using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Tests.EditMode.GridRenders
{
    [TestFixture]
    public class PerCellGridRenderer_EditModeTests
    {
        [Test]
        public void AddStates_AddsAndGetCellStates_ReturnsExpected()
        {
            var prefabGO = new GameObject("cell_pref_em");
            prefabGO.AddComponent<MeshRenderer>();
            var cv = prefabGO.AddComponent<CellView>();

            var parent = new GameObject("parent_em");
            var rendererGO = new GameObject("renderer_em");
            var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
            renderer.SetPrefab(cv);
            renderer.SetParent(parent);
            renderer.SetMaterials(new List<CellMaterial>());
            renderer.Render(3,3,1f, Vector3.zero, 0f);

            var coords = new List<Vector2Int> { new Vector2Int(1,1) };
            renderer.AddStates(coords, CellState.reachableCell);

            var states = renderer.GetCellStates(new Vector2Int(1,1));
            bool found = false;
            foreach (var s in states) if (s == CellState.reachableCell) { found = true; break; }

            Assert.IsTrue(found);

            Object.DestroyImmediate(prefabGO);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(rendererGO);
        }

        [Test]
        public void ClearAllStates_RemovesStates()
        {
            var prefabGO = new GameObject("cell_pref_em2");
            prefabGO.AddComponent<MeshRenderer>();
            var cv = prefabGO.AddComponent<CellView>();

            var parent = new GameObject("parent_em2");
            var rendererGO = new GameObject("renderer_em2");
            var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
            renderer.SetPrefab(cv);
            renderer.SetParent(parent);
            renderer.SetMaterials(new List<CellMaterial>());
            renderer.Render(3,3,1f, Vector3.zero, 0f);

            var coords = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,1) };
            renderer.AddStates(coords, CellState.reachableCell);
            renderer.ClearAllStates();

            var s0 = renderer.GetCellStates(new Vector2Int(0,0));
            Assert.IsTrue(s0 == null || s0.Length == 0);

            Object.DestroyImmediate(prefabGO);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(rendererGO);
        }

        [Test]
        public void AddState_AddsSingleState()
        {
            var prefabGO = new GameObject("cell_pref_em3");
            prefabGO.AddComponent<MeshRenderer>();
            var cv = prefabGO.AddComponent<CellView>();

            var parent = new GameObject("parent_em3");
            var rendererGO = new GameObject("renderer_em3");
            var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
            renderer.SetPrefab(cv);
            renderer.SetParent(parent);
            renderer.SetMaterials(new List<CellMaterial>());
            renderer.Render(2,2,1f, Vector3.zero, 0f);

            var coord = new Vector2Int(0,1);
            renderer.AddState(coord, CellState.accessibleRoutePoint);

            var states = renderer.GetCellStates(coord);
            bool found = false;
            if (states != null)
            {
                foreach (var s in states) if (s == CellState.accessibleRoutePoint) { found = true; break; }
            }

            Assert.IsTrue(found);

            Object.DestroyImmediate(prefabGO);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(rendererGO);
        }

        [Test]
        public void RemoveState_RemovesState()
        {
            var prefabGO = new GameObject("cell_pref_em4");
            prefabGO.AddComponent<MeshRenderer>();
            var cv = prefabGO.AddComponent<CellView>();

            var parent = new GameObject("parent_em4");
            var rendererGO = new GameObject("renderer_em4");
            var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
            renderer.SetPrefab(cv);
            renderer.SetParent(parent);
            renderer.SetMaterials(new List<CellMaterial>());
            renderer.Render(2,2,1f, Vector3.zero, 0f);

            var coord = new Vector2Int(1,0);
            renderer.AddState(coord, CellState.reachableCell);
            renderer.RemoveState(coord, CellState.reachableCell);

            var states = renderer.GetCellStates(coord);
            Assert.IsTrue(states == null || states.Length == 0);

            Object.DestroyImmediate(prefabGO);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(rendererGO);
        }

        [Test]
        public void RemoveStates_RemovesMultipleStates()
        {
            var prefabGO = new GameObject("cell_pref_em5");
            prefabGO.AddComponent<MeshRenderer>();
            var cv = prefabGO.AddComponent<CellView>();

            var parent = new GameObject("parent_em5");
            var rendererGO = new GameObject("renderer_em5");
            var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
            renderer.SetPrefab(cv);
            renderer.SetParent(parent);
            renderer.SetMaterials(new List<CellMaterial>());
            renderer.Render(3,3,1f, Vector3.zero, 0f);

            var coords = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(2,2) };
            renderer.AddStates(coords, CellState.reachableCell);

            // now remove (remove all occurrences of the state)
            renderer.RemoveStates(CellState.reachableCell);

            foreach (var c in coords)
            {
                var s = renderer.GetCellStates(c);
                Assert.IsTrue(s == null || s.Length == 0);
            }

            Object.DestroyImmediate(prefabGO);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(rendererGO);
        }
    }
}
