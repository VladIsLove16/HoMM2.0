using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Combat
{
    public class TurnSystemIntegrationTests
    {
        private UnitStats _statsBlue;
        private UnitStats _statsRed;
        private UnitModel _blue1;
        private UnitModel _red1;
        private UnitViewModel _blueVm;
        private UnitViewModel _redVm;
        private UnitView3D _blueView;
        private UnitView3D _redView;
        private GameObject _goBlue;
        private GameObject _goRed;
        private TurnSystem _turnSystem;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _statsBlue = ScriptableObject.CreateInstance<UnitStats>();
            _statsBlue.Health = 100; _statsBlue.MaxHealth = 100; _statsBlue.InvulnerableEffects = new List<StatusEffectType>();
            _statsRed = ScriptableObject.CreateInstance<UnitStats>();
            _statsRed.Health = 100; _statsRed.MaxHealth = 100; _statsRed.InvulnerableEffects = new List<StatusEffectType>();

            _blue1 = new UnitModel(_statsBlue, UnitType.Archer, 0, 0, 1, true);
            _red1 = new UnitModel(_statsRed, UnitType.Archer, 1, 0, 1, false);
            _blueVm = new UnitViewModel(_blue1, new MaterialProvider());
            _redVm = new UnitViewModel(_red1, new MaterialProvider());

            _goBlue = new GameObject("BlueUnit", typeof(Animator));
            _goRed = new GameObject("RedUnit", typeof(Animator));
            _blueView = _goBlue.AddComponent<UnitView3D>();
            _redView = _goRed.AddComponent<UnitView3D>();
            _blueView.Init(_blueVm);
            _redView.Init(_redVm);

            _turnSystem = new TurnSystem();
            _turnSystem.AddCombatUnit(_blue1);
            _turnSystem.AddCombatUnit(_red1);
            _turnSystem.RunBattle();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_statsBlue != null) Object.DestroyImmediate(_statsBlue);
            if (_statsRed != null) Object.DestroyImmediate(_statsRed);
            if (_goBlue != null) Object.DestroyImmediate(_goBlue);
            if (_goRed != null) Object.DestroyImmediate(_goRed);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DeadUnit_DoesNotReceiveTurn_And_RemainsInactive()
        {
            // Убиваем активного юнита (скорее всего, первым ходит _blue1)
            _blue1.RecieveDamage(new DamageContext(1000, DamageType.physical, null));
            yield return new WaitForSeconds(0.2f);

            // GameObject синего должен деактивироваться
            Assert.That(_goBlue.activeSelf, Is.False);

            // Завершаем текущий ход, следующий должен быть не у синего
            _turnSystem.EndTurn();
            yield return null;

            Assert.That(_turnSystem.ActiveObject.Value, Is.EqualTo(_red1));
        }

        [UnityTest]
        public IEnumerator DeadUnit_DoesNotTriggerMoveHighlights_OnItsTurn()
        {
            // Убиваем синего
            _blue1.RecieveDamage(new DamageContext(1000, DamageType.physical, null));
            yield return new WaitForSeconds(0.2f);

            // Имитируем завершение хода — следующий должен быть у красного
            _turnSystem.EndTurn();
            yield return null;

            Assert.That(_turnSystem.ActiveObject.Value, Is.EqualTo(_red1));

            // На этом месте обычно вызывалась подсветка клеток для активного —
            // Проверяем, что у синего (мертвого) вообще не выполняется логика начала хода.
            // Косвенно: он деактивирован и не ActiveObject
            Assert.That(_goBlue.activeSelf, Is.False);
        }
    }
}
