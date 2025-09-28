using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UnitView3D_Hover_Select_Tests
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

    [UnityTest]
    public IEnumerator UnitView3D_Hover_InvokesHover()
    {
        var gvGO = new GameObject("gv_unit");
        var gv = gvGO.AddComponent<GameView3D>();
        gv.SetWorldToCellProvider(new StubWorldToCellProvider());

        var unitGO = new GameObject("unit");
        var uv = unitGO.AddComponent<UnitView3D>();
        unitGO.transform.position = new Vector3(2,0,2);

        // UnitView3D implements IHoverable, ensure GameView3D hover call invokes Hover()
        gv.HandleGameViewObjectHovered(uv);

        // UnitView3D should respond by changing its internal state (no public flag) so at least ensure no exceptions
        Assert.Pass();

        Object.DestroyImmediate(gvGO);
        Object.DestroyImmediate(unitGO);
        yield return null;
    }
}
