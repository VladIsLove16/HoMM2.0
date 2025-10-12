using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Tests.EditMode.GridRenders
{
    [TestFixture]
    public class CellView_MaterialAssignment_Tests
    {
        [Test]
        public void RoutePointMaterial_AssignedFromMaterialsDict()
        {
            var prefab = new GameObject("cell_mat");
            prefab.AddComponent<MeshRenderer>();
            var cv = prefab.AddComponent<CellView>();

            var route = new GameObject("route"); route.transform.SetParent(prefab.transform, false);
            var rr = route.AddComponent<MeshRenderer>();
            cv.SetRoutePointRenderer(rr);

            var mat = new Material(Shader.Find("Standard"));
            var cm = new CellMaterial (CellState.accessibleRoutePoint, mat );
            var dict = new Dictionary<CellState, CellMaterial> { { cm.CellState, cm } };

            cv.SetMaterialsDictionary(dict);
            cv.AddState(CellState.accessibleRoutePoint);

            Assert.IsTrue(rr.gameObject.activeSelf);
            Assert.AreEqual(mat, rr.material);

            Object.DestroyImmediate(prefab);
        }
    }
}
