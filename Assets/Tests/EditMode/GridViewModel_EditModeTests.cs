using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.ViewModels
{
    [TestFixture]
    public class GridViewModel_EditModeTests
    {
        private GridViewModel _vm;
        private ActionResolver _resolver;
        private MovementSystem _movement;
        private GameModel _model;
        private TurnStateViewModel _turnState;

        [SetUp]
        public void SetUp()
        {
            var dict = TestDataFactory.CreateSingleUnitData(UnitType.Archer);
            _movement = new MovementSystem();
            _model = new GameModel(new UnitModelFactory(dict), _movement);
            _resolver = new ActionResolver(_model, _movement);
            var turnService = new TurnService(new TurnQueue());
            _turnState = new TurnStateViewModel(turnService);
            _vm = new GridViewModel(_turnState, _resolver, _movement, _model);
            _model.InitializeGrid(2, 2);
        }

        [TearDown]
        public void TearDown()
        {
            _turnState?.Dispose();
        }

        [Test]
        public void ActionResolved_ForwardsPreviewToSubscribers()
        {
            var target = new Vector2Int(1, 1);
            var expected = new PreviewResult();
            expected.Add(CellState.hovered, new[] { target });

            PreviewResult received = null;
            _vm.PreviewChanged += preview => received = preview;

            var handler = new FakeHandler(expected);
            RaiseActionResolved(handler, new ActionContext());

            Assert.That(received, Is.Not.Null);
            var dict = received.ToDictionary();
            Assert.That(dict.ContainsKey(CellState.hovered));
            Assert.That(dict[CellState.hovered], Does.Contain(target));
        }

        private void RaiseActionResolved(IActionHandler handler, ActionContext ctx)
        {
            var field = typeof(ActionResolver).GetField("ActionResolved", BindingFlags.Instance | BindingFlags.NonPublic);
            var callback = (Action<(IActionHandler, ActionContext)>)field?.GetValue(_resolver);
            callback?.Invoke((handler, ctx));
        }

        private class FakeHandler : IActionHandler
        {
            private readonly PreviewResult _preview;

            public FakeHandler(PreviewResult preview)
            {
                _preview = preview;
            }

            public ActionType ActionType => ActionType.Move;

            public void Execute(ActionContext ctx)
            {
            }

            public bool CanExecute(ActionContext ctx) => true;

            public PreviewResult GetPreview(ActionContext ctx) => _preview;
        }
    }
}
