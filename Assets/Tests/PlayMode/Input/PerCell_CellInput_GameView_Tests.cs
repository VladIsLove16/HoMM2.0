using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PerCell_CellInput_GameView_Tests
{
    [UnityTest]
    public IEnumerator PerCellGridRenderer_AddHoverState_TogglesHoverRenderer()
    {
        var prefabGO = new GameObject("CellPrefab");
        var baseRenderer = prefabGO.AddComponent<MeshRenderer>();
        prefabGO.AddComponent<MeshFilter>().mesh = new Mesh();
        var cellView = prefabGO.AddComponent<CellView>();

        // add child hover renderer
        var hoverGO = new GameObject("hover");
        hoverGO.transform.SetParent(prefabGO.transform, false);
        var hoverRenderer = hoverGO.AddComponent<MeshRenderer>();
        cellView.SetHoverRenderer(hoverRenderer);

        var parent = new GameObject("cellsParent");
        var rendererGO = new GameObject("renderer");
        var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
        renderer.SetPrefab(cellView);
        renderer.SetParent(parent);
        renderer.SetMaterials(new List<CellMaterial>());

        renderer.Render(2,2,1f, Vector3.zero, 0f);

    // add hovered state at 0,0
    var point = new Vector2Int(0,0);
    renderer.AddStates(new List<Vector2Int> { point }, CellState.hovered);

        // find instantiated cell under parent
        var childCell = parent.GetComponentInChildren<CellView>(true);
        Assert.IsNotNull(childCell);

        var states = childCell.GetStates();
        Assert.Contains(CellState.hovered, states);

        // hover renderer should be active
        var hoverChild = childCell.transform.Find("hover");
        Assert.IsNotNull(hoverChild);
        Assert.IsTrue(hoverChild.gameObject.activeSelf);

        Object.DestroyImmediate(prefabGO);
        Object.DestroyImmediate(parent);
        Object.DestroyImmediate(rendererGO);
        yield return null;
    }

    [UnityTest]
    public IEnumerator PerCellGridRenderer_RoutePoint_MaterialAssigned()
    {
        var prefabGO = new GameObject("CellPrefab2");
        prefabGO.AddComponent<MeshRenderer>();
        prefabGO.AddComponent<MeshFilter>().mesh = new Mesh();
        var cellView = prefabGO.AddComponent<CellView>();

        var routeGO = new GameObject("route"); routeGO.transform.SetParent(prefabGO.transform, false);
        var routeRenderer = routeGO.AddComponent<MeshRenderer>();
        cellView.SetRoutePointRenderer(routeRenderer);

        var mat = new Material(Shader.Find("Standard"));
        var cm = new CellMaterial(CellState.accessibleRoutePoint, mat );

        var parent = new GameObject("cellsParent2");
        var rendererGO = new GameObject("renderer2");
        var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
        renderer.SetPrefab(cellView);
        renderer.SetParent(parent);
        renderer.SetMaterials(new List<CellMaterial> { cm });

        renderer.Render(1,1,1f, Vector3.zero, 0f);

    renderer.AddStates(new List<Vector2Int> { new Vector2Int(0,0) }, CellState.accessibleRoutePoint);

        var childCell = parent.GetComponentInChildren<CellView>(true);
        Assert.IsNotNull(childCell);
        var routeChild = childCell.transform.Find("route");
        Assert.IsNotNull(routeChild);
        Assert.IsTrue(routeChild.gameObject.activeSelf);

        // Material assigned to renderer
        var mr = routeChild.GetComponent<MeshRenderer>();
        Assert.AreEqual(mat, mr.sharedMaterial);

        Object.DestroyImmediate(prefabGO);
        Object.DestroyImmediate(parent);
        Object.DestroyImmediate(rendererGO);
        yield return null;
    }

    class StubWorldToCellProvider : IWorldToCellProvider
    {
        public Vector3 ToWorld(int x, int y) => new Vector3(x, 0, y);
        public bool ToGrid(Vector3 position, out Vector2Int coords)
        {
            coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
            return true;
        }
        public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
        {
            var key = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
            coords = new KeyValuePair<Vector2Int, Vector2Int>(key, new Vector2Int(key.x + 1, key.y));
            return true;
        }
    }

    class FakeGameViewObject : MonoBehaviour, IGameViewObject, IHoverable
    {
        public bool Hovered;
        public bool Selectable => true;
        public bool IsHoverable => true;
        public bool IsSelectable => true;

        public void Hover() { Hovered = true; }
        public void Unhover() { Hovered = false; }
    }

    [UnityTest]
    public IEnumerator GameView3D_HandleHover_InvokesHoverAndCallsDelegate()
    {
        var gvGO = new GameObject("gv");
        var gv = gvGO.AddComponent<GameView3D>();
        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

        KeyValuePair<Vector2Int, Vector2Int> received = default;
        gv.TestHandleCellHovered = (pair) => received = pair;

        var objGO = new GameObject("obj");
        var fake = objGO.AddComponent<FakeGameViewObject>();
        objGO.transform.position = new Vector3(2,0,3);

        gv.HandleGameViewObjectHovered(fake);

        Assert.IsTrue(fake.Hovered);
        Assert.AreEqual(new Vector2Int(2,3), received.Key);

        Object.DestroyImmediate(gvGO);
        Object.DestroyImmediate(objGO);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameView3D_HandleSelected_CallsDelegate()
    {
        var gvGO = new GameObject("gv2");
        var gv = gvGO.AddComponent<GameView3D>();
        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

        KeyValuePair<Vector2Int, Vector2Int> received = default;
        gv.TestHandleCellSelected = (pair) => received = pair;

        var objGO = new GameObject("obj2");
        var fake = objGO.AddComponent<FakeGameViewObject>();
        objGO.transform.position = new Vector3(1,0,4);

        gv.HandleGameViewObjectSelected(fake);

        Assert.AreEqual(new Vector2Int(1,4), received.Key);

        Object.DestroyImmediate(gvGO);
        Object.DestroyImmediate(objGO);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CellInputHandler_ProcessGameViewObject_Reflection_ForwardsToGameView()
    {
        var ciGO = new GameObject("ci");
        var cih = ciGO.AddComponent<CellInputHandler>();

        var gvGO = new GameObject("gv3");
        var gv = gvGO.AddComponent<GameView3D>();
        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

        bool hovered = false;
        gv.TestHandleCellHovered = (pair) => hovered = true;

        // Set internal game view field via reflection (TestSetGameView is internal). Use reflection to find the internal method.
        var setGameView = typeof(CellInputHandler).GetMethod("TestSetGameView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (setGameView != null)
        {
            setGameView.Invoke(cih, new object[] { gv });
        }

        // Find nested enum InputInteractionType and get Hover value
        var nested = typeof(CellInputHandler).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
        System.Type interactionType = null;
        foreach (var t in nested) if (t.Name.Contains("InputInteractionType")) interactionType = t;

        var proc = typeof(CellInputHandler).GetMethod("TestProcessGameViewObject", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var fakeGO = new GameObject("fakeProc");
        var fake = fakeGO.AddComponent<FakeGameViewObject>();

        if (proc != null && interactionType != null)
        {
            var hoverEnum = System.Enum.Parse(interactionType, "Hover");
            proc.Invoke(cih, new object[] { fake, hoverEnum });
        }

        Assert.IsTrue(hovered);

        Object.DestroyImmediate(ciGO);
        Object.DestroyImmediate(gvGO);
        Object.DestroyImmediate(fakeGO);
        yield return null;
    }
}
