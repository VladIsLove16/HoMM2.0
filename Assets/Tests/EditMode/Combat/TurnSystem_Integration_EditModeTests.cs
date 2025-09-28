using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace Tests.EditMode.Combat
{
    [TestFixture]
    public class TurnSystem_Integration_EditModeTests
    {
        private TurnSystem _turnSystem;
        private CombatStub _blueUnit;
        private CombatStub _redUnit;

        [SetUp]
        public void SetUp()
        {
            _turnSystem = new TurnSystem();
            _blueUnit = new CombatStub(Team.Blue);
            _redUnit = new CombatStub(Team.Red);
            _turnSystem.AddCombatUnit(_blueUnit);
            _turnSystem.AddCombatUnit(_redUnit);
        }

        [TearDown]
        public void TearDown()
        {
            if (_blueUnit?.Stats != null)
            {
                GameObject.DestroyImmediate(_blueUnit.Stats);
            }

            if (_redUnit?.Stats != null)
            {
                GameObject.DestroyImmediate(_redUnit.Stats);
            }
        }

        [Test]
        public void RunBattle_SelectsFirstAvailableUnit()
        {
            _turnSystem.RunBattle();

            Assert.That(_turnSystem.ActiveObject.Value, Is.SameAs(_blueUnit));
            Assert.That(_blueUnit.TurnsTaken, Is.EqualTo(1));
        }

        [Test]
        public void EndTurn_SwitchesToNextAliveUnit()
        {
            _turnSystem.RunBattle();
            _turnSystem.EndTurn();

            Assert.That(_turnSystem.ActiveObject.Value, Is.SameAs(_redUnit));
            Assert.That(_redUnit.TurnsTaken, Is.EqualTo(1));
        }

        [Test]
        public void RemoveCombatUnit_EndsBattleWhenOneTeamRemains()
        {
            _turnSystem.RunBattle();
            _turnSystem.RemoveCombatUnit(_redUnit);

            Assert.That(_turnSystem.CombatUnits, Has.Count.EqualTo(1));
            Assert.That(_turnSystem.CombatUnits[0], Is.SameAs(_blueUnit));
        }

        private class CombatStub : ICombatObject
        {
            public CombatStub(Team team)
            {
                Team = team;
                Position = Vector2Int.zero;
                Stats = ScriptableObject.CreateInstance<UnitStats>();
                Stats.Health = 100;
                Stats.MaxHealth = 100;
                Stats.Damage = 10;
            }

            public Team Team { get; }
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;
            public void TakeTurn() => TurnsTaken++;
            public void EndTurn() => TurnsEnded++;
            public UnitType UnitType => UnitType.Archer;
            public UnitStats Stats { get; }
            public Action<ICombatObject> Died { get; set; }
            public int TurnsTaken { get; private set; }
            public int TurnsEnded { get; private set; }
        }
    }
}

