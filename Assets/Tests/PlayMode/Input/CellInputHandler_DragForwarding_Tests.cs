using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CellInputHandler_DragForwarding_Tests
{
    class FakeGameView3D : GameView3D
    {
        public List<Vector3> dragEvents = new List<Vector3>();
        public override void UpdateDrag(Vector3 pos)
        {
            dragEvents.Add(pos);
        }
    }

    [UnityTest]
    public IEnumerator Drag_Is_Forwarded_To_GameView3D()
    {
        var go = new GameObject("cih");
        var cih = go.AddComponent<CellInputHandler>();
        var fake = new GameObject("fakeGV").AddComponent<FakeGameView3D>();

        // use internal test helpers if available
        #if UNITY_EDITOR
        cih.TestSetGameView(fake);
        #else
        // best-effort: set via property if exists
        #endif

        var unit = new GameObject("unit").AddComponent<UnitView3D>();

        // simulate drag update processing
        cih.TestProcessGameViewObject(unit, CellInputHandler.InputInteractionType.BeginDrag);
        cih.TestProcessGameViewObject(unit, CellInputHandler.InputInteractionType.DragUpdate);
        cih.TestProcessGameViewObject(unit, CellInputHandler.InputInteractionType.EndDrag);

        // FakeGameView3D should have recorded drag updates
        Assert.IsTrue(fake.dragEvents.Count >= 1);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(fake.gameObject);
        Object.DestroyImmediate(unit.gameObject);
        yield return null;
    }
}
