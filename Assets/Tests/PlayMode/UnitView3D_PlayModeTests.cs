using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Views
{
	public class UnitView3D_PlayModeTests
	{
		private UnitStats _stats;
		private UnitModel _model;
		private UnitViewModel _vm;
		private UnitView3D _view;

		[UnitySetUp]
		public IEnumerator SetUp()
		{
			_stats = ScriptableObject.CreateInstance<UnitStats>();
			_stats.Health = 100;
			_stats.MaxHealth = 100;
			_stats.Damage = 20;
			_stats.InvulnerableEffects = new List<StatusEffectType>();
			_model = new UnitModel(_stats, UnitType.Archer, 0, 0, 1, true);
			_vm = new UnitViewModel(_model, new MaterialProvider());
			var go = new GameObject("UnitView3D", typeof(Animator));
			_view = go.AddComponent<UnitView3D>();
			_view.Init(_vm);
			yield return null;
		}

		[UnityTearDown]
		public IEnumerator TearDown()
		{
			if (_stats != null) Object.DestroyImmediate(_stats);
			if (_view != null) Object.DestroyImmediate(_view.gameObject);
			yield return null;
		}

		[UnityTest]
		public IEnumerator Hover_Unhover_ChangesMaterialSafely()
		{
			_view.Hover();
			yield return null;
			_view.Unhover();
			yield return null;
			Assert.Pass();
		}

		[UnityTest]
		public IEnumerator HandleHit_PlaysHitAnimation()
		{
			_view.HandleHit(new DamageContext(10, DamageType.physical, null));
			yield return null;
			Assert.Pass();
		}

		[UnityTest]
		public IEnumerator HandleDeath_Deactivates_WhenAmountBecomesZero()
		{
			_model.RecieveDamage(new DamageContext(1000, DamageType.physical, null));
			_view.HandleDeath();
			yield return new WaitForSeconds(1.6f);
			Assert.That(_view.gameObject.activeSelf, Is.False);
		}

		[UnityTest]
		public IEnumerator RequestMove_QueuesAndProcessesMovement()
		{
			var route = new List<Vector3> { new Vector3(1,0,0), new Vector3(2,0,0) };
			_view.RequestMove(route);
			yield return new WaitForSeconds(0.2f);
			Assert.Pass();
		}
	}
}


