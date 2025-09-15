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
        private TurnSystem _turnSystem;

        [SetUp]
        public void SetUp()
        {
            _statsBlue = ScriptableObject.CreateInstance<UnitStats>();
            _statsBlue.Health = 100; _statsBlue.MaxHealth = 100; _statsBlue.Damage = 25; _statsBlue.InvulnerableEffects = new List<StatusEffectType>();

            _statsRed = ScriptableObject.CreateInstance<UnitStats>();
            _statsRed.Health = 100; _statsRed.MaxHealth = 100; _statsRed.Damage = 25; _statsRed.InvulnerableEffects = new List<StatusEffectType>();

            _blue1 = new UnitModel(_statsBlue, UnitType.Archer, 0, 0, 1, true);
            _red1 = new UnitModel(_statsRed, UnitType.Witch, 1, 0, 1, false);

            _turnSystem = new TurnSystem();
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
            // Arrange
            _turnSystem.AddCombatUnit(_blue1);
            _turnSystem.AddCombatUnit(_red1);

            // Симулируем смерть синего до старта боя
            _turnSystem.RemoveCombatUnit(_blue1);

            // Act
            _turnSystem.RunBattle();

            // Assert — активным должен стать живой противник
            Assert.That(_turnSystem.ActiveObject.Value, Is.EqualTo(_red1));
        }

        [Test]
        public void EndTurn_DoesNotGiveTurnToRemovedUnit()
        {
            // Arrange
            _turnSystem.AddCombatUnit(_blue1);
            _turnSystem.AddCombatUnit(_red1);
            _turnSystem.RunBattle();

            // Первый ход у первого в очереди (синий)
            var first = _turnSystem.ActiveObject.Value;
            Assert.That(first, Is.EqualTo(_blue1));

            // Симулируем смерть синего в середине раунда
            _turnSystem.RemoveCombatUnit(_red1);

            // Act — заканчиваем ход, следующий ход не должен достаться удаленному
            _turnSystem.EndTurn();

            // Assert — активный это красный, не удаленный
            Assert.That(_turnSystem.ActiveObject.Value, Is.EqualTo(_blue1));
        }

        [Test]
        public void MultipleTurns_SkipAllRemovedUnits()
        {
            // Arrange
            var blue2Stats = ScriptableObject.CreateInstance<UnitStats>();
            blue2Stats.Health = 100; blue2Stats.MaxHealth = 100; blue2Stats.InvulnerableEffects = new List<StatusEffectType>();
            var blue2 = new UnitModel(blue2Stats, UnitType.Archer, 2, 0, 1, true);

            _turnSystem.AddCombatUnit(_blue1);
            _turnSystem.AddCombatUnit(blue2);
            _turnSystem.AddCombatUnit(_red1);
            _turnSystem.RunBattle();

            // Удаляем синего первого
            _turnSystem.RemoveCombatUnit(_blue1);

            // Крутим несколько ходов — удаленный больше не должен становиться активным
            for (int i = 0; i < 4; i++)
            {
                _turnSystem.EndTurn();
                Assert.That(_turnSystem.ActiveObject.Value, Is.Not.EqualTo(_blue1));
            }

            Object.DestroyImmediate(blue2Stats);
        }

        [Test]
        public void UpdateBattleState_ReturnsWin_WhenOnlyOneTeamLeft_AfterRemovals()
        {
            // Arrange
            _turnSystem.AddCombatUnit(_blue1);
            _turnSystem.AddCombatUnit(_red1);
            _turnSystem.RunBattle();

            // Удаляем всех красных (симуляция смерти)
            _turnSystem.RemoveCombatUnit(_red1);

            // Act
            var state = _turnSystem.BattleState;

            // Assert — бой завершен победой оставшейся команды
            Assert.That(state == BattleState.blueTeamWins || state == BattleState.redTeamWins, Is.True);
        }
    }
}
