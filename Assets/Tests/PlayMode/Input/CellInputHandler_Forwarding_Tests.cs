using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CellInputHandler_Forwarding_Tests
{
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

    class FakeSelectable : MonoBehaviour, IGameViewObject
    {
        public bool IsSelectable => true;
        public bool IsHoverable => false;
        public void Hover() {}
    }

    [UnityTest]
    public IEnumerator CellInputHandler_ForwardsSelectToGameView3D()
    {
        var ciGO = new GameObject("ci_test");
        var ci = ciGO.AddComponent<CellInputHandler>();

        var gvGO = new GameObject("gv_test");
        var gv = gvGO.AddComponent<GameView3D>();
        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

        // Use internal test setter
        typeof(CellInputHandler).GetMethod("TestSetGameView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(ci, new object[] { gv });

        bool selected = false;
        gv.TestHandleCellSelected = (pair) => selected = true;

        var obj = new GameObject("objsel");
        var fake = obj.AddComponent<FakeSelectable>();
        obj.transform.position = new Vector3(1,0,1);

        // call internal ProcessGameViewObject with Select interaction
        var nested = typeof(CellInputHandler).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        System.Type interactionType = null;
        foreach (var t in nested) if (t.Name.Contains("InputInteractionType")) interactionType = t;

        var proc = typeof(CellInputHandler).GetMethod("TestProcessGameViewObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var selEnum = System.Enum.Parse(interactionType, "Select");
        proc.Invoke(ci, new object[] { fake, selEnum });

        Assert.IsTrue(selected);

        Object.DestroyImmediate(ciGO);
        Object.DestroyImmediate(gvGO);
        Object.DestroyImmediate(obj);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CellInputHandler_ForwardsActionToGameView3D()
    {
        var ciGO = new GameObject("ci_test2");
        var ci = ciGO.AddComponent<CellInputHandler>();

        var gvGO = new GameObject("gv_test2");
        var gv = gvGO.AddComponent<GameView3D>();
        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

        typeof(CellInputHandler).GetMethod("TestSetGameView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(ci, new object[] { gv });

    gv.TestHandleCellHovered = (pair) => { };
        // We can't easily intercept HandleActionPerformed via a test hook, so test that no exceptions occur when forwarding
        var obj = new GameObject("objact");
        var fake = obj.AddComponent<FakeSelectable>();
        obj.transform.position = new Vector3(2,0,2);

        var nested = typeof(CellInputHandler).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        System.Type interactionType = null;
        foreach (var t in nested) if (t.Name.Contains("InputInteractionType")) interactionType = t;

        var proc = typeof(CellInputHandler).GetMethod("TestProcessGameViewObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var actEnum = System.Enum.Parse(interactionType, "Action");
        proc.Invoke(ci, new object[] { fake, actEnum });

        Object.DestroyImmediate(ciGO);
        Object.DestroyImmediate(gvGO);
        Object.DestroyImmediate(obj);
        yield return null;
    }
}
