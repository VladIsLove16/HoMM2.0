using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Tests.Common;

namespace Tests.PlayMode.Units
{
    [TestFixture]
    public class UnitView3D_PlayModeDeathTests
    {
        [UnityTest]
        public IEnumerator HandleDeath_DeactivatesAfterDelay_WhenStackEmpties()
        {
            var stats = CreateStats();
            var model = new UnitModel(stats, UnitType.Archer, 0, 0, 1, true);
            var viewModel = new UnitViewModel(model, new TestWorldToCellProvider());
            var go = new GameObject("UnitView3D_PlayMode", typeof(Animator));
            var view = go.AddComponent<UnitView3D>();
            InjectMaterialProvider(view);
            view.Init(viewModel);

            model.RecieveDamage(new DamageContext(999, DamageType.physical, null));
            view.HandleDeath();
            CompleteAnimation(view, UnitAnimationEvent.DieFinished);
            yield return null;

            Assert.That(go.activeSelf, Is.False);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(stats);
        }

        [UnityTest]
        public IEnumerator HandleDeath_WithRemainingUnits_StaysActive()
        {
            var stats = CreateStats();
            var model = new UnitModel(stats, UnitType.Archer, 0, 0, 2, true);
            var viewModel = new UnitViewModel(model, new TestWorldToCellProvider());
            var go = new GameObject("UnitView3D_PlayMode_Remaining", typeof(Animator));
            var view = go.AddComponent<UnitView3D>();
            InjectMaterialProvider(view);
            view.Init(viewModel);

            model.RecieveDamage(new DamageContext(150, DamageType.physical, null));
            view.HandleDeath();
            CompleteAnimation(view, UnitAnimationEvent.HitFinished);
            yield return null;

            Assert.That(go.activeSelf, Is.True);
            Assert.That(model.Amount.Value, Is.EqualTo(1));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(stats);
        }

        private static UnitStats CreateStats()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 100;
            stats.MaxHealth = 100;
            stats.Damage = 25;
            stats.InvulnerableEffects = new List<StatusEffectType>();
            return stats;
        }

        private static void InjectMaterialProvider(UnitView3D view)
        {
            var providerField = typeof(UnitView3D).GetField("_teamMaterials", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            providerField?.SetValue(view, new StubMaterialProvider());
        }

        private class StubMaterialProvider : IMaterialProvider
        {
            public Material GetTeamMaterial(Team team) => CreateMaterial($"Team_{team}");
            public Material GetHoveredTeamMaterial(Team team) => CreateMaterial($"Hovered_{team}");
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


        private static void CompleteAnimation(UnitView3D view, UnitAnimationEvent evt)
        {
            var controller = view.GetComponent<UnitAnimatorController>();
            controller?.DispatchAnimationEvent((int)evt);
        }
    }
}
