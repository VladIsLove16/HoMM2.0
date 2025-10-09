using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Combat
{
    [TestFixture]
    public class TurnSystemTests
    {
        private UnitStats _statsBlue;
        private UnitStats _statsRed;
        private UnitModel _blue1;
        private UnitModel _red1;
        private TurnService _turnService;

        [SetUp]
        public void SetUp()
        {
            _statsBlue = ScriptableObject.CreateInstance<UnitStats>();
            _statsBlue.Health = 100; _statsBlue.MaxHealth = 100; _statsBlue.Damage = 25; _statsBlue.InvulnerableEffects = new List<StatusEffectType>();

            _statsRed = ScriptableObject.CreateInstance<UnitStats>();
            _statsRed.Health = 100; _statsRed.MaxHealth = 100; _statsRed.Damage = 25; _statsRed.InvulnerableEffects = new List<StatusEffectType>();

            _blue1 = new UnitModel(_statsBlue, UnitType.Archer, 0, 0, 1, true);
            _red1 = new UnitModel(_statsRed, UnitType.Witch, 1, 0, 1, false);

            _turnService = new TurnService(new TurnQueue());
        }

        [TearDown]
        public void TearDown()
        {
            if (_statsBlue != null) Object.DestroyImmediate(_statsBlue);
            if (_statsRed != null) Object.DestroyImmediate(_statsRed);
        }

        [Test]
        public void RunBattle_SkipsDeadUnits_IfRemovedBeforeStart()
        {
            _turnService.AddCombatUnit(_blue1);
            _turnService.AddCombatUnit(_red1);

            _turnService.RemoveCombatUnit(_blue1);

            _turnService.RunBattle();

            Assert.That(_turnService.ActiveObject, Is.EqualTo(_red1));
        }

        [Test]
        public void EndTurn_DoesNotGiveTurnToRemovedUnit()
        {
            _turnService.AddCombatUnit(_blue1);
            _turnService.AddCombatUnit(_red1);
            _turnService.RunBattle();

            var first = _turnService.ActiveObject;
            Assert.That(first, Is.EqualTo(_blue1));

            _turnService.RemoveCombatUnit(_red1);

            _turnService.EndTurn();

            Assert.That(_turnService.ActiveObject, Is.EqualTo(_blue1));
        }

        [Test]
        public void MultipleTurns_SkipAllRemovedUnits()
        {
            var blue2Stats = ScriptableObject.CreateInstance<UnitStats>();
            blue2Stats.Health = 100; blue2Stats.MaxHealth = 100; blue2Stats.InvulnerableEffects = new List<StatusEffectType>();
            var blue2 = new UnitModel(blue2Stats, UnitType.Archer, 2, 0, 1, true);

            _turnService.AddCombatUnit(_blue1);
            _turnService.AddCombatUnit(blue2);
            _turnService.AddCombatUnit(_red1);
            _turnService.RunBattle();

            _turnService.RemoveCombatUnit(_blue1);

            for (int i = 0; i < 4; i++)
            {
                _turnService.EndTurn();
                Assert.That(_turnService.ActiveObject, Is.Not.EqualTo(_blue1));
            }

            Object.DestroyImmediate(blue2Stats);
        }

        [Test]
        public void UpdateBattleState_ReturnsWin_WhenOnlyOneTeamLeft_AfterRemovals()
        {
            _turnService.AddCombatUnit(_blue1);
            _turnService.AddCombatUnit(_red1);
            _turnService.RunBattle();

            _turnService.RemoveCombatUnit(_red1);

            var state = _turnService.BattleState;

            Assert.That(state == BattleState.blueTeamWins || state == BattleState.redTeamWins, Is.True);
        }
    }
}
