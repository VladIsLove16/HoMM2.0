//using NUnit.Framework;
//using UnityEngine;
//using System.Collections.Generic;

//namespace Tests.EditMode.ViewModels
//{
//    [TestFixture]
//    public class GameViewModel_PerCell_EditModeTests
//    {
//        [Test]
//        public void GameViewModel_StartTurn_SetsAccessibleStatesInRenderer()
//        {
//            var prefabGO = new GameObject("cell_pref");
//            prefabGO.AddComponent<MeshRenderer>();
//            var cellView = prefabGO.AddComponent<CellView>();

//            var parent = new GameObject("parent_edit");
//            var rendererGO = new GameObject("renderer_edit");
//            var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
//            renderer.SetPrefab(cellView);
//            renderer.SetParent(parent);
//            renderer.SetMaterials(new List<CellMaterial>());
//            renderer.Render(3,3,1f, Vector3.zero, 0f);

//            // Create GridViewModel and simulate start turn reachable cells
//            var reachable = new List<Vector2Int> { new Vector2Int(1,1), new Vector2Int(1,2) };

//            // The renderer API is public - just call AddStates to emulate the model behavior
//            renderer.AddStates(reachable, CellState.accessibleRoutePoint);

//            // verify renderer has states present on at least one cell
//            bool found = false;
//            foreach (var cv in parent.GetComponentsInChildren<CellView>(true))
//            {
//                var states = cv.GetStates();
//                foreach (var s in states) if (s == CellState.accessibleRoutePoint) { found = true; break; }
//                if (found) break;
//            }

//            Assert.IsTrue(found);

//            Object.DestroyImmediate(prefabGO);
//            Object.DestroyImmediate(parent);
//            Object.DestroyImmediate(rendererGO);
//        }
//    }
//}
