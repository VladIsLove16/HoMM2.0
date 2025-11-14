using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Tests.Common;

namespace Tests.EditMode.GridContents.Units
{
    [TestFixture]
    public class UnitIntegration_EditModeTests
    {
        private UnitStats _stats;
        private UnitModel _model;
        private UnitViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _stats = ScriptableObject.CreateInstance<UnitStats>();
            _stats.Health = 100;
            _stats.MaxHealth = 100;
            _stats.Damage = 25;
            _stats.InvulnerableEffects = new List<StatusEffectType>();

            _model = new UnitModel(_stats, UnitType.Archer, 0, 0, 3, true);
            _viewModel = new UnitViewModel(_model, new TestWorldToCellProvider());
        }

        [TearDown]
        public void TearDown()
        {
            if (_stats != null)
            {
                Object.DestroyImmediate(_stats);
            }
        }

        [Test]
        public void ViewModel_HealthRatio_UpdatesAfterDamage()
        {
            _model.RecieveDamage(new DamageContext(40, DamageType.physical, null));

            Assert.That(_viewModel.HealthRatio, Is.EqualTo(0.6f));
            Assert.That(_viewModel.Health, Is.EqualTo(60));
        }

        [Test]
        public void Model_MoveByRoute_RaisesMovedEvent()
        {
            var route = new List<Vector2Int> { new Vector2Int(1, 1), new Vector2Int(2, 2) };
            List<Vector2Int> receivedRoute = null;
            _model.MovedByRoute += r => receivedRoute = r;

            _model.MoveByRoute(route);

            Assert.That(receivedRoute, Is.EqualTo(route));
            Assert.That(_model.Position.Value, Is.EqualTo(route[^1]));
        }

        [Test]
        public void Model_SendDamage_ReturnsStackScaledDamage()
        {
            var target = new MockDamagable();
            var attackContext = new AttackContext(target);

            var damage = _model.SendDamage(attackContext);

            Assert.That(damage.DamageAmount, Is.EqualTo(_stats.Damage * _model.Amount.Value));
            Assert.That(damage.Source, Is.EqualTo(_model));
        }

        private class MockDamagable : IDamagable
        {
            public bool IsBlueTeam => false;
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType => GridContentType.unit;
            public Team Team => Team.Red;
            public void RecieveDamage(DamageContext ctx) { }
            public void SimulateRecieveDamage(DamageContext ctx) { }
        }
    }
}
