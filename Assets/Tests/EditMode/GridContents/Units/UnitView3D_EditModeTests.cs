using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
using Tests.Common;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitView3D_EditModeTests
    {
        private UnitStats _stats;
        private UnitModel _model;
        private UnitViewModel _viewModel;
        private IWorldToCellProvider _worldProvider;
        private UnitView3D _view;
        private GameObject _viewGO;
        private StubMaterialProvider _materialProvider;

        [SetUp]
        public void SetUp()
        {
            _stats = ScriptableObject.CreateInstance<UnitStats>();
            _stats.Health = 100;
            _stats.MaxHealth = 100;
            _stats.Damage = 20;
            _stats.InvulnerableEffects = new List<StatusEffectType>();

            _model = new UnitModel(_stats, UnitType.Archer, 0, 0, 1, true);
            _worldProvider = new TestWorldToCellProvider();
            _viewModel = new UnitViewModel(_model, _worldProvider);

            _viewGO = new GameObject("UnitView3D", typeof(Animator));
            _view = _viewGO.AddComponent<UnitView3D>();

            _materialProvider = new StubMaterialProvider();
            InjectMaterialProvider(_view, _materialProvider);

            _view.Init(_viewModel);
        }

        [TearDown]
        public void TearDown()
        {
            _view?.Dispose();
            if (_viewGO != null)
            {
                Object.DestroyImmediate(_viewGO);
            }

            if (_stats != null)
            {
                Object.DestroyImmediate(_stats);
            }
        }

        [Test]
        public void Hover_RequestsHoveredMaterial()
        {
            _materialProvider.Reset();

            _view.Hover();

            Assert.That(_materialProvider.RequestedHoveredTeams.Count, Is.EqualTo(1));
            Assert.That(_materialProvider.RequestedHoveredTeams[0], Is.EqualTo(Team.Blue));
        }

        [Test]
        public void HandleHit_EnqueuesHitAnimation()
        {
            var queue = GetActionQueue(_view);
            queue.Clear();

            SetIsExecuting(_view, true);
            _view.HandleHit(Vector3.forward);

            Assert.That(queue.Count, Is.EqualTo(1));
        }

        [Test]
        public void HandleDeath_DeactivatesView_WhenStackEmpty()
        {
            var queue = GetActionQueue(_view);
            queue.Clear();

            _model.RecieveDamage(new DamageContext(1000, DamageType.physical, null));
            SetIsExecuting(_view, true);
            _view.HandleDeath();

            Assert.That(queue.Count, Is.EqualTo(1), "Death action should be enqueued");

            var action = queue.Dequeue();
            var dispatched = false;
            RunEnumerator(action, () =>
            {
                if (!dispatched)
                {
                    CompleteAnimation(_view, UnitAnimationEvent.DieFinished);
                    dispatched = true;
                }
            });

            Assert.That(_view.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void HandleDeath_WithRemainingStack_LeavesViewActive()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 100;
            stats.MaxHealth = 100;
            stats.Damage = 20;
            stats.InvulnerableEffects = new List<StatusEffectType>();

            var model = new UnitModel(stats, UnitType.Archer, 0, 0, 2, true);
            var vm = new UnitViewModel(model, new TestWorldToCellProvider());
            var go = new GameObject("UnitView3D_Remaining", typeof(Animator));
            var view = go.AddComponent<UnitView3D>();
            var provider = new StubMaterialProvider();
            InjectMaterialProvider(view, provider);
            view.Init(vm);

            try
            {
                var queue = GetActionQueue(view);
                queue.Clear();

                model.RecieveDamage(new DamageContext(150, DamageType.physical, null));
                SetIsExecuting(view, true);
                view.HandleDeath();

                Assert.That(queue.Count, Is.EqualTo(1));

                var action = queue.Dequeue();
                var dispatched = false;
                RunEnumerator(action, () =>
                {
                    if (!dispatched)
                    {
                        CompleteAnimation(view, UnitAnimationEvent.HitFinished);
                        dispatched = true;
                    }
                });

                Assert.That(view.gameObject.activeSelf, Is.True);
                Assert.That(model.Amount.Value, Is.EqualTo(1));
            }
            finally
            {
                view.Dispose();
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(stats);
            }
        }

        private static void InjectMaterialProvider(UnitView3D view, IMaterialProvider provider)
        {
            var providerField = typeof(UnitView3D).GetField("_teamMaterials", BindingFlags.NonPublic | BindingFlags.Instance);
            providerField?.SetValue(view, provider);
        }

        private static Queue<IEnumerator> GetActionQueue(UnitView3D view)
        {
            var field = typeof(UnitView3D).GetField("actionQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Queue<IEnumerator>)field?.GetValue(view);
        }

        private static void SetIsExecuting(UnitView3D view, bool value)
        {
            var field = typeof(UnitView3D).GetField("isExecuting", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(view, value);
        }

        private static void RunEnumerator(IEnumerator enumerator, System.Action onYield = null)
        {
            while (enumerator.MoveNext())
            {
                onYield?.Invoke();
            }
        }

        private static void CompleteAnimation(UnitView3D view, UnitAnimationEvent evt)
        {
            var controller = view.GetComponent<UnitAnimatorController>();
            controller?.DispatchAnimationEvent((int)evt);
        }

        private class StubMaterialProvider : IMaterialProvider
        {
            public readonly List<Team> RequestedHoveredTeams = new();

            public void Reset() => RequestedHoveredTeams.Clear();

            public Material GetTeamMaterial(Team team) => CreateMaterial($"Team_{team}");

            public Material GetHoveredTeamMaterial(Team team)
            {
                RequestedHoveredTeams.Add(team);
                return CreateMaterial($"Hovered_{team}");
            }

            public Material GetBlueTeamMaterial() => CreateMaterial("Blue");
            public Material GetHoveredBlueTeamMaterial() => CreateMaterial("HoveredBlue");
            public Material GetRedTeamMaterial() => CreateMaterial("Red");
            public Material GetHoveredRedTeamMaterial() => CreateMaterial("HoveredRed");

            private static Material CreateMaterial(string name)
            {
                var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Sprites/Default");
                return new Material(shader) { name = name };
            }
        }
    }
}
