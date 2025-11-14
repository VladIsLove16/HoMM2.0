//using System.Collections;
//using NUnit.Framework;
//using UnityEngine;
//using UnityEngine.TestTools;
//using static Tests.PlayMode.Grid.PerCellGridRenderer_PlayModeTests;

//public class Hover_State_Transition_Tests
//{
//    [UnityTest]
//    public IEnumerator HoveredMyTurn_DoesNotActivateHoverRendererUnlessHoveredIsSet()
//    {
//        //var prefab = new GameObject("cell_hov");
//        //prefab.AddComponent<MeshRenderer>();
//        //var cv = prefab.AddComponent<CellView>();

//        //var hoverChild = new GameObject("hover"); hoverChild.transform.SetParent(prefab.transform, false);
//        //var hr = hoverChild.AddComponent<MeshRenderer>();
//        //cv.SetHoverRenderer(hr);

//        //var parent = new GameObject("parent_hov");
//        //var rendererGO = new GameObject("renderer_hov");
//        //var rendererVM = new TestGridViewModel();
//        //var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
//        //rendererVM.Width = 3;
//        //rendererVM.Height = 3;
//        //renderer.Bind(rendererVM);
//        //renderer.SetPrefab(cv);
//        //renderer.SetParent(parent);
//        //renderer.SetMaterials(new System.Collections.Generic.List<CellMaterial>());
//        //renderer.Render();

//        //// Add hoveredMyTurn state only
//        //renderer.
//        //rendererVM.AddState(new Vector2Int(0,0), CellState.hoveredMyTurn);

//        //var instantiated = parent.GetComponentInChildren<CellView>(true);
//        //Assert.IsNotNull(instantiated);
//        //var hover = instantiated.transform.Find("hover");
//        //Assert.IsNotNull(hover);
//        //// hovered renderer should not be active because CellView reacts only to CellState.hovered
//        //Assert.IsFalse(hover.gameObject.activeSelf);

//        //// Now add plain hovered and check it activates
//        //renderer.AddState(new Vector2Int(0,0), CellState.hovered);
//        //Assert.IsTrue(hover.gameObject.activeSelf);

//        //Object.DestroyImmediate(prefab);
//        //Object.DestroyImmediate(parent);
//        //Object.DestroyImmediate(rendererGO);
//        //yield return null;
//    }
//}
