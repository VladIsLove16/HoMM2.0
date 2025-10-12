using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Units
{
	[TestFixture]
	public class UnitModel_EditModeTests
	{
		private UnitStats _stats;
		private UnitModel _unit;

		[SetUp]
		public void SetUp()
		{
			_stats = ScriptableObject.CreateInstance<UnitStats>();
			_stats.Health = 100;
			_stats.MaxHealth = 100;
			_stats.Damage = 25;
			_stats.MoveSpeed = 3;
			_stats.AttackRange = 2;
			_stats.InvulnerableEffects = new List<StatusEffectType>();

			_unit = new UnitModel(_stats, UnitType.Archer, 1, 2, 3, true);
		}

		[TearDown]
		public void TearDown()
		{
			if (_stats != null) UnityEngine.Object.DestroyImmediate(_stats);
		}

		[Test]
		public void MoveByRoute_UpdatesPosition_And_RaisesMovedEvent()
		{
			var route = new List<Vector2Int> { new Vector2Int(1, 2), new Vector2Int(2, 2), new Vector2Int(3, 2) };
			List<Vector2Int> received = null;
			_unit.MovedByRoute += r => received = r;

			_unit.MoveByRoute(route);

			Assert.That(_unit.Position.Value, Is.EqualTo(new Vector2Int(3, 2)));
			Assert.That(received, Is.EqualTo(route));
		}

		[Test]
		public void RecieveDamage_PartialDamage_DecreasesHealthOnly()
		{
			var ctx = new DamageContext(30, DamageType.physical, null);
			_unit.RecieveDamage(ctx);

			Assert.That(_unit.ModifiedStats.Health, Is.EqualTo(70));
			Assert.That(_unit.Amount.Value, Is.EqualTo(3));
		}

		[Test]
		public void RecieveDamage_ExactKill_ConsumesOneStack_ResetsHealth()
		{
			var ctx = new DamageContext(100, DamageType.physical, null);
			_unit.RecieveDamage(ctx);

			Assert.That(_unit.Amount.Value, Is.EqualTo(2));
			Assert.That(_unit.ModifiedStats.Health, Is.EqualTo(_unit.BaseUnitStats.MaxHealth));
		}

		[Test]
		public void RecieveDamage_Overkill_KillsAll_InvokesDied()
		{
			bool died = false;
			_unit.Died += () => died = true;

			var ctx = new DamageContext(10000, DamageType.physical, null);
			_unit.RecieveDamage(ctx);

			Assert.That(_unit.Amount.Value, Is.EqualTo(0));
			Assert.That(_unit.ModifiedStats.Health, Is.EqualTo(0));
			Assert.That(died, Is.True);
		}

		[Test]
		public void TakeTurn_RaisesTurnStarted()
		{
			bool raised = false;
			_unit.TurnStarted += () => raised = true;

			_unit.TakeTurn();

			Assert.That(raised, Is.True);
		}
	}
}


