using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Units
{
    [TestFixture]
    public class UnitView3D_PlayModeAsyncTests
    {
        private static readonly FieldInfo ActionQueueField = typeof(UnitView3D).GetField("actionQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo IsExecutingField = typeof(UnitView3D).GetField("isExecuting", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly List<GameObject> _objects = new();
        private readonly List<ScriptableObject> _assets = new();

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var go in _objects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            _objects.Clear();

            foreach (var asset in _assets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }
            _assets.Clear();

            yield return null;
        }

        [UnityTest]
        public IEnumerator RequestMove_CompletesMovementAcrossRoute()
        {
            var fixture = CreateViewFixture();
            var route = new List<Vector3>
            {
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 2f)
            };

            fixture.View.Move(route);
            yield return WaitUntilPosition(fixture.View.transform, route.Last());
            yield return WaitForQueueToDrain(fixture.View);

            Assert.That(Vector3.Distance(fixture.View.transform.position, route.Last()), Is.LessThan(0.05f));
        }

        [UnityTest]
        public IEnumerator RequestMove_SequentialRoutesReachBothDestinations()
        {
            var fixture = CreateViewFixture();
            var first = new Vector3(0f, 0f, 1f);
            var second = new Vector3(0f, 0f, 2f);
            fixture.View.Move(new List<Vector3> { first });
            fixture.View.Move(new List<Vector3> { second });

            yield return WaitUntilPosition(fixture.View.transform, first);
            yield return WaitUntilPosition(fixture.View.transform, second);
            yield return WaitForQueueToDrain(fixture.View);

            Assert.That(Vector3.Distance(fixture.View.transform.position, second), Is.LessThan(0.05f));
        }

        [UnityTest]
        public IEnumerator HandleHit_CompletesQueuedAnimation()
        {
            var fixture = CreateViewFixture();
            fixture.View.HandleHit(new DamageContext(10, DamageType.physical, null));

            Assert.That(GetIsExecuting(fixture.View), Is.True);
            yield return WaitForQueueToDrain(fixture.View);
            Assert.That(GetIsExecuting(fixture.View), Is.False);
        }

        [UnityTest]
        public IEnumerator HandleAttack_CompletesQueuedAnimation()
        {
            var fixture = CreateViewFixture();
            fixture.View.HandleAttack(new DamageContext(5, DamageType.physical, null));

            Assert.That(GetIsExecuting(fixture.View), Is.True);
            yield return WaitForQueueToDrain(fixture.View);
            Assert.That(GetIsExecuting(fixture.View), Is.False);
        }

        [UnityTest]
        public IEnumerator HoverAndUnhover_UpdateMaterials()
        {
            var fixture = CreateViewFixture();
            var renderer = fixture.Renderer;
            var stub = (StubMaterialProvider)fixture.Provider;

            fixture.View.Hover();
            yield return null;
            Assert.That(renderer.material.name, Does.Contain(stub.HoverMaterials[Team.Blue].name));

            fixture.View.Unhover();
            yield return null;
            Assert.That(renderer.material.name, Does.Contain("TeamBlue"));
        }

        [UnityTest]
        public IEnumerator HandleDeath_WithRemainingStack_KeepsViewActive()
        {
            var fixture = CreateViewFixture(amount: 2);
            fixture.Model.RecieveDamage(new DamageContext(150, DamageType.physical, null));
            fixture.View.HandleDeath();

            yield return WaitForQueueToDrain(fixture.View);
            Assert.That(fixture.View.gameObject.activeSelf, Is.True);
            Assert.That(fixture.Model.Amount.Value, Is.EqualTo(1));
        }

        private IEnumerator WaitForQueueToDrain(UnitView3D view)
        {
            float timeout = 3f;
            while ((GetQueueCount(view) > 0 || GetIsExecuting(view)) && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator WaitUntilPosition(Transform transform, Vector3 target)
        {
            float timeout = 3f;
            while (Vector3.Distance(transform.position, target) > 0.05f && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        private static int GetQueueCount(UnitView3D view)
        {
            var queue = (Queue<IEnumerator>)ActionQueueField!.GetValue(view);
            return queue.Count;
        }

        private static bool GetIsExecuting(UnitView3D view)
        {
            return (bool)IsExecutingField!.GetValue(view);
        }

        private ViewFixture CreateViewFixture(int amount = 1)
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 100;
            stats.MaxHealth = 100;
            stats.Damage = 20;
            stats.MoveSpeed = 5;
            stats.InvulnerableEffects = new List<StatusEffectType>();
            _assets.Add(stats);

            var model = new UnitModel(stats, UnitType.Archer, 0, 0, amount, Team.Blue);
            var viewModel = new UnitViewModel(model);

            var go = new GameObject("UnitView3D", typeof(Animator));
            var renderer = go.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = new Mesh();
            _objects.Add(go);

            var view = go.AddComponent<UnitView3D>();
            var provider = new StubMaterialProvider();
            typeof(UnitView3D).GetField("_teamMaterials", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(view, provider);

            go.SetActive(true);
            view.Init(viewModel);

            return new ViewFixture(view, model, renderer, provider);
        }

        private class ViewFixture
        {
            public ViewFixture(UnitView3D view, UnitModel model, SkinnedMeshRenderer renderer, StubMaterialProvider provider)
            {
                View = view;
                Model = model;
                Renderer = renderer;
                Provider = provider;
            }

            public UnitView3D View { get; }
            public UnitModel Model { get; }
            public SkinnedMeshRenderer Renderer { get; }
            public IMaterialProvider Provider { get; }
        }

        private class StubMaterialProvider : IMaterialProvider
        {
            public readonly Dictionary<Team, Material> HoverMaterials = new();
            private readonly Dictionary<Team, Material> _teamMaterials = new();

            public StubMaterialProvider()
            {
                HoverMaterials[Team.Blue] = CreateMaterial("HoverBlue");
                HoverMaterials[Team.Red] = CreateMaterial("HoverRed");
                _teamMaterials[Team.Blue] = CreateMaterial("TeamBlue");
                _teamMaterials[Team.Red] = CreateMaterial("TeamRed");
            }

            public Material GetTeamMaterial(Team team) => _teamMaterials[team];
            public Material GetHoveredTeamMaterial(Team team) => HoverMaterials[team];
            public Material GetBlueTeamMaterial() => _teamMaterials[Team.Blue];
            public Material GetHoveredBlueTeamMaterial() => HoverMaterials[Team.Blue];
            public Material GetRedTeamMaterial() => _teamMaterials[Team.Red];
            public Material GetHoveredRedTeamMaterial() => HoverMaterials[Team.Red];

            private Material CreateMaterial(string name)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
                return new Material(shader) { name = name };
            }
        }
    }
}
