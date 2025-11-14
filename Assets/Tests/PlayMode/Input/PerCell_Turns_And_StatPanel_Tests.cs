//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using NUnit.Framework;
//using UnityEngine;
//using UnityEngine.TestTools;

//public class PerCell_Turns_And_StatPanel_Tests
//{
//    class StubWorldToCellProvider : IWorldToCellProvider
//    {
//        public Vector3 ToWorld(int x, int y) => new Vector3(x, 0, y);
//        public bool ToGrid(Vector3 position, out Vector2Int coords)
//        {
//            coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
//            return true;
//        }
//        public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
//        {
//            var key = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
//            coords = new KeyValuePair<Vector2Int, Vector2Int>(key, new Vector2Int(key.x + 1, key.y));
//            return true;
//        }
//    }

//    //[UnityTest]
//    //public IEnumerator TurnStart_HighlightsReachableCells()
//    //{
//    //    // Create PerCellGridRenderer and a cell prefab
//    //    //var prefabGO = new GameObject("cellPrefab_turn");
//    //    //prefabGO.AddComponent<MeshRenderer>();
//    //    //prefabGO.AddComponent<MeshFilter>().mesh = new Mesh();
//    //    //var cellView = prefabGO.AddComponent<CellView>();
//    //    //var hoverGO = new GameObject("hover"); hoverGO.transform.SetParent(prefabGO.transform, false);
//    //    //var hoverRenderer = hoverGO.AddComponent<MeshRenderer>();
//    //    //cellView.SetHoverRenderer(hoverRenderer);

//    //    //var parent = new GameObject("cellsParent_turn");
//    //    //var rendererGO = new GameObject("renderer_turn");
//    //    //var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
//    //    //renderer.SetPrefab(cellView);
//    //    //renderer.SetParent(parent);
//    //    //renderer.SetMaterials(new List<CellMaterial>());
//    //    //renderer.Render(4,4,1f, Vector3.zero, 0f);

//    //    //// Simulate accessible cells for a unit at 1,1 (reachable = 3 cells for test)
//    //    //var reachable = new List<Vector2Int> { new Vector2Int(2,1), new Vector2Int(1,2), new Vector2Int(0,1) };
//    //    //renderer.AddStates(reachable, CellState.accessibleRoutePoint);

//    //    //// ensure route points are active
//    //    //var found = parent.GetComponentInChildren<CellView>(true);
//    //    //Assert.IsNotNull(found);

//    //    //bool anyRouteActive = false;
//    //    //foreach (var mr in parent.GetComponentsInChildren<MeshRenderer>(true))
//    //    //{
//    //    //    if (mr.gameObject.name.ToLower().Contains("route") && mr.gameObject.activeSelf) anyRouteActive = true;
//    //    //}

//    //    //Assert.IsTrue(anyRouteActive);

//    //    //Object.DestroyImmediate(prefabGO);
//    //    //Object.DestroyImmediate(parent);
//    //    //Object.DestroyImmediate(rendererGO);
//    //    var moveSys = new MovementSystem();
//    //    var gameModel = new GameModel(new(), moveSys);
//    //    var turnSys = new TurnSystem();
//    //    GridViewModel gridViewModel = new(turnSys, new(gameModel, moveSys), moveSys, gameModel);
//    //    PerCellGridRenderer perCellGridRenderer = new();
//    //    perCellGridRenderer.Bind(gridViewModel);
//    //    List<Vector2Int> reachableCells = new List<Vector2Int>();
//    //    var perCellGridRendererreachableCells = perCellGridRenderer.GetCells(CellState.reachableCell);
//    //    foreach(var cell in reachableCells)
//    //    {
//    //        Assert.IsTrue(perCellGridRendererreachableCells.Contains(cell));
//    //    }
//    //    yield return null;
//    //}

//    [UnityTest]
//    public IEnumerator EnemyTurn_ShowsOnlyEnemyHints()
//    {
//        // We'll simulate a renderer that only shows enemy hints by toggling a flag via states
//        var prefabGO = new GameObject("cellPrefab_enemy");
//        prefabGO.AddComponent<MeshRenderer>();
//        prefabGO.AddComponent<MeshFilter>().mesh = new Mesh();
//        var cellView = prefabGO.AddComponent<CellView>();
//        var routeGO = new GameObject("route"); routeGO.transform.SetParent(prefabGO.transform, false);
//        var routeRenderer = routeGO.AddComponent<MeshRenderer>();
//        cellView.SetRoutePointRenderer(routeRenderer);

//        var parent = new GameObject("cellsParent_enemy");
//        var rendererGO = new GameObject("renderer_enemy");
//        var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
//        renderer.SetPrefab(cellView);
//        renderer.SetParent(parent);
//        renderer.SetMaterials(new List<CellMaterial>());
//        renderer.Render(3,3,1f, Vector3.zero, 0f);

//        // Simulate enemy-only hints: assign accessibleRoutePoint (enemy)
//    renderer.AddStates(new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0) }, CellState.enemyReachableCell);

//        // Ensure enemy hint objects are active and others not
//        var anyEnemyHintActive = false;
//        foreach (var cw in parent.GetComponentsInChildren<CellView>(true))
//        {
//            var states = cw.GetStates();
//            foreach (var s in states) if (s == CellState.enemyReachableCell) anyEnemyHintActive = true;
//        }

//        Assert.IsTrue(anyEnemyHintActive);

//        Object.DestroyImmediate(prefabGO);
//        Object.DestroyImmediate(parent);
//        Object.DestroyImmediate(rendererGO);
//        yield return null;
//    }

//    [UnityTest]
//    public IEnumerator Selection_OpensStatPanel()
//    {
//        // Create GameView3D and a fake selectable object that opens a stat panel when selected
//        var gvGO = new GameObject("gv_stat");
//        var gv = gvGO.AddComponent<GameView3D>();
//        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

//        // Create stat panel (simple GameObject) and hook into GameView3D via TestHandleCellSelected to activate it
//        var statPanel = new GameObject("StatPanel");
//        statPanel.SetActive(false);

//        gv.TestHandleCellSelected = (pair) => { statPanel.SetActive(true); };

//        var objGO = new GameObject("unitStatObj");
//        var fake = objGO.AddComponent<FakeSelectableAndHoverable>();
//        objGO.transform.position = new Vector3(2,0,2);

//        gv.HandleGameViewObjectSelected(fake);

//        Assert.IsTrue(statPanel.activeSelf);

//        Object.DestroyImmediate(gvGO);
//        Object.DestroyImmediate(objGO);
//        Object.DestroyImmediate(statPanel);
//        yield return null;
//    }

//    class FakeSelectableAndHoverable : MonoBehaviour, IGameViewObject, IHoverable
//    {
//        public bool Hovered;
//        public bool Selectable => true;
//        public bool IsHoverable => true;
//        public bool IsSelectable => true;
//        public void Hover() { Hovered = true; }
//        public void Unhover() { Hovered = false; }
//    }
//}
