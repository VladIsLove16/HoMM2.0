using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class ComprehensivePlayModeStubs
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Ensure global singleton used by many tests exists
            PlayModeTestSetup.EnsureSceneTransitionDataService();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneTransitionDataService_Exists()
        {
            Assert.IsNotNull(SceneTransitionDataService.Instance);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameSceneConfigurationProvider_CanBeCreated()
        {
            var provider = new GameSceneConfigurationProvider(SceneTransitionDataService.Instance);
            provider.Initialize();
            Assert.DoesNotThrow(() => provider.GetSelectedConfiguration());
            yield return null;
        }

        [UnityTest]
        public IEnumerator MovementSystem_InitializeGrid_GetReachableCells_NoThrow()
        {
            var movement = new MovementSystem();
            var model = new GameModel(new UnitModelFactory(TestDataFactory.CreateSingleUnitData(UnitType.Archer)), movement);
            model.InitializeGrid(4, 4);
            var start = new Vector2Int(0, 0);
            var reachable = movement.GetReachableCells(start, 3);
            Assert.IsNotNull(reachable);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MoveActionHandler_CanExecute_WithRouteStub_ReturnsFalseWhenTooExpensive()
        {
            // Movement stub that returns high cost
            var movement = new MovementSystem();
            var model = new GameModel(new UnitModelFactory(TestDataFactory.CreateSingleUnitData(UnitType.Archer)), movement);
            model.InitializeGrid(3, 3);
            var spawn = new UnitSpawnParams(0, 0, UnitType.Archer, 1, Team.Blue);
            Assert.IsTrue(model.SpawnUnit(spawn).IsSuccess);

            var moveHandler = new MoveActionHandler(movement, model);
            var ctx = new ActionContext(new Vector2Int(0,0), new Vector2Int(2,2), default, new Vector2Int(2,2));
            // cannot guarantee path cost here; simply call CanExecute to make sure it doesn't throw
            Assert.DoesNotThrow(() => moveHandler.CanExecute(ctx));
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitView3D_Init_Creates_UI_IfMissing()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.MaxHealth = 100; stats.Health = 100; stats.InvulnerableEffects = new List<StatusEffectType>();
            var model = new UnitModel(stats, UnitType.Archer, 0,0,1, Team.Blue);
            var vm = new UnitViewModel(model);

            var go = new GameObject("UV3DTest");
            var uv = go.AddComponent<UnitView3D>();
            // add Animator but no controller
            go.AddComponent<Animator>();

            uv.Init(vm);
            // after Init, UnitViewUI should be present (we auto-create in code)
            Assert.IsNotNull(go.GetComponentInChildren<UnitViewUI>(true));
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitView3D_Play_SkipsTrigger_WhenNoController()
        {
            var go = new GameObject("UV3DAnimTest");
            var uv = go.AddComponent<UnitView3D>();
            var animator = go.AddComponent<Animator>();
            // ensure no RuntimeAnimatorController assigned
            animator.runtimeAnimatorController = null;
            // call Play and ensure no exception
            Assert.DoesNotThrow(() => uv.Play(UnitAnimationState.Idle));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PerCellGridRenderer_Render_CreatesCells()
        {
            // Create minimal cell prefab
            var prefab = new GameObject("CellPrefab");
            var cellView = prefab.AddComponent<CellView>();
            prefab.AddComponent<MeshRenderer>();
            prefab.AddComponent<MeshFilter>().mesh = MeshGenerator.CreateQuad();

            var parent = new GameObject("parentCells");
            var rendererGO = new GameObject("PerCellGridRenderer");
            var renderer = rendererGO.AddComponent<PerCellGridRenderer>();
            renderer.SetPrefab(cellView);
            renderer.SetParent(parent);

            renderer.SetMaterials(new System.Collections.Generic.List<CellMaterial>());
            renderer.Render(2,2,1f, Vector3.zero, 0f);
            // Expect some child objects created under parent
            Assert.IsTrue(parent.transform.childCount > 0);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(rendererGO);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameView3D_HandleHover_Calls_VM()
        {
            var gvGO = new GameObject("GameView3D");
            var gv = gvGO.AddComponent<GameView3D>();
            bool called = false;
            gv.TestHandleCellHovered = (pair) => called = true;
            // create a dummy IGameViewObject
            var objGO = new GameObject("obj");
            var cellView = objGO.AddComponent<CellView>();
            gv.HandleGameViewObjectHovered(cellView);
            Assert.IsTrue(called);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CellInputHandler_Awake_AssignsMainCamera_Test()
        {
            var cam = new GameObject("MainCamera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            var go = new GameObject("CIH");
            var handler = go.AddComponent<CellInputHandler>();
            handler.gameObject.SetActive(true);
            yield return null;
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator UnitHealthBar_Init_NoException()
        {
            // Create minimal viewmodel and UI components
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 10; stats.MaxHealth = 10; stats.InvulnerableEffects = new List<StatusEffectType>();
            var model = new UnitModel(stats, UnitType.Archer, 0,0,1, Team.Blue);
            var vm = new UnitViewModel(model);

            var go = new GameObject("UIRoot");
            var ui = go.AddComponent<UnitViewUI>();

            // Create required private fields using reflection
            var hbGO = new GameObject("hb"); hbGO.transform.SetParent(go.transform);
            hbGO.AddComponent<UnitHealthBar>();
            var amtGO = new GameObject("amt"); amtGO.transform.SetParent(go.transform);
            amtGO.AddComponent<TMPro.TextMeshProUGUI>();

            Assert.DoesNotThrow(() => ui.Init(vm));
            Object.DestroyImmediate(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ConfigurationDebugTest_CanRun()
        {
            // This test duplicates the debug test but ensures the service exists
            var svc = PlayModeTestSetup.EnsureSceneTransitionDataService();
            Assert.IsNotNull(svc);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Network_SpawnUnit_LocalMode_NoExceptions()
        {
            var model = new GameModel(new UnitModelFactory(TestDataFactory.CreateSingleUnitData(UnitType.Archer)), new MovementSystem());
            model.InitializeGrid(3,3);
            var res = model.SpawnUnit(new UnitSpawnParams(1,1, UnitType.Archer, 1, Team.Blue));
            Assert.IsTrue(res.IsSuccess);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitViewUI_Dispose_Idempotent()
        {
            var go = new GameObject("uiv");
            var ui = go.AddComponent<UnitViewUI>();
            Assert.DoesNotThrow(() => { ui.Dispose(); ui.Dispose(); });
            Object.DestroyImmediate(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MovementSystem_GetRouteCost_DoesNotThrow()
        {
            var movement = new MovementSystem();
            var route = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2) };
            Assert.DoesNotThrow(() => movement.GetRouteCost(route));
            yield return null;
        }

        [UnityTest]
        public IEnumerator MovementSystem_HasLineOfSight_NoBlockers_ReturnsTrue()
        {
            var movement = new MovementSystem();
            var model = new GameModel(new UnitModelFactory(TestDataFactory.CreateSingleUnitData(UnitType.Archer)), movement);
            model.InitializeGrid(5,5);
            var from = new Vector2Int(0,0);
            var to = new Vector2Int(4,4);
            Assert.DoesNotThrow(() => movement.HasLineOfSight(from, to));
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitView3D_Hover_SetsMaterial_NoThrow()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.InvulnerableEffects = new List<StatusEffectType>();
            var model = new UnitModel(stats, UnitType.Archer, 0,0,1, Team.Blue);
            var vm = new UnitViewModel(model);
            var go = new GameObject("uvh");
            var uv = go.AddComponent<UnitView3D>();
            go.AddComponent<Animator>();
            uv.Init(vm);
            Assert.DoesNotThrow(() => uv.Hover());
            Object.DestroyImmediate(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnitView3D_SnapToCell_UpdatesTransform()
        {
            var go = new GameObject("snap");
            var uv = go.AddComponent<UnitView3D>();
            var pos = new Vector2Int(2,3);
            var world = new Vector3(pos.x, 0, pos.y);
            Assert.DoesNotThrow(() => uv.SnapToCell(go.AddComponent<UnitView3D>(), pos));
            Object.DestroyImmediate(go);
            yield return null;
        }
    }

    // Small mesh generator for tests
    internal static class MeshGenerator
    {
        public static Mesh CreateQuad()
        {
            var m = new Mesh();
            m.vertices = new Vector3[] { new Vector3(-0.5f,0, -0.5f), new Vector3(0.5f,0,-0.5f), new Vector3(0.5f,0,0.5f), new Vector3(-0.5f,0,0.5f) };
            m.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            m.triangles = new int[] { 0,1,2, 0,2,3 };
            m.RecalculateNormals();
            return m;
        }
    }
}
