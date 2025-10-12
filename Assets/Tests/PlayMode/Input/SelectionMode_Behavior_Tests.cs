using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SelectionMode_Behavior_Tests
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
    public IEnumerator SelectionMode_GridOnly_DoesNotPickUnitWhenNotPresent()
    {
        var ciGO = new GameObject("ci_sel");
        var ci = ciGO.AddComponent<CellInputHandler>();

        var gvGO = new GameObject("gv_sel");
        var gv = gvGO.AddComponent<GameView3D>();
        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

        // attach test game view
        typeof(CellInputHandler).GetMethod("TestSetGameView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(ci, new object[] { gv });

        // set GridOnly mode
        typeof(CellInputHandler).GetMethod("TestSetSelectionMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(ci, new object[] { CellInputHandler.SelectionMode.GridOnly });

        bool selected = false;
        gv.TestHandleCellSelected = (pair) => selected = true;

        // No selectable object at position -> call should still forward a grid selection only when hit exists
        var probe = new GameObject("probe");
        probe.transform.position = new Vector3(0,0,0);

        // Directly call internal processor
        var nested = typeof(CellInputHandler).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        System.Type interactionType = null;
        foreach (var t in nested) if (t.Name.Contains("InputInteractionType")) interactionType = t;
        var proc = typeof(CellInputHandler).GetMethod("TestProcessGameViewObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        // Passing null to simulate grid hit
        var hoverEnum = System.Enum.Parse(interactionType, "Select");
        proc.Invoke(ci, new object[] { null, hoverEnum });

        // nothing selected because no hit object
        Assert.IsFalse(selected);

        Object.DestroyImmediate(ciGO);
        Object.DestroyImmediate(gvGO);
        Object.DestroyImmediate(probe);
        yield return null;
    }
}
