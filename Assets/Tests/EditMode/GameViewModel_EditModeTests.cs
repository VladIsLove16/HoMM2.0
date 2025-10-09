using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Tests.EditMode.Units;

namespace Tests.EditMode.ViewModels
{
    [TestFixture]
    public class GameViewModel_EditModeTests
    {
        private GameModel _model;
        private MovementSystem _movement;
        private TurnService _turnService;
        private TurnStateViewModel _turnState;
        private GameViewModel _vm;
        private ActionResolver _resolver;
        private FakeGameCommandExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            var dict = TestDataFactory.CreateSingleUnitData(UnitType.Archer);
            _movement = new MovementSystem();
            _model = new GameModel(new UnitModelFactory(dict), _movement);
            _turnService = new TurnService(new TurnQueue());
            _turnState = new TurnStateViewModel(_turnService);
            _resolver = new ActionResolver(_model, _movement);
            _executor = new FakeGameCommandExecutor();
            _vm = new GameViewModel(_model, _movement, _executor, _turnState, _resolver);
            _model.InitializeGrid(3, 3);
        }

        [Test]
        public void GridInitialized_Event_Fires()
        {
            int width = 0, height = 0;
            _vm.GridInited += (x,y) => { width = x; height = y; };
            _model.InitializeGrid(5, 4);
            Assert.That((width, height), Is.EqualTo((5, 4)));
        }

        [Test]
        public void HandleCellHovered_PublishesMovePreviewAndHoverHighlight()
        {
            SpawnActiveUnit(new Vector2Int(0, 0));

            var previews = new List<PreviewResult>();
            _vm.PreviewChanged += previews.Add;

            var target = new Vector2Int(1, 0);
            var coords = new KeyValuePair<Vector2Int, Vector2Int>(target, target);

            _vm.HandleCellHovered(coords);

            Assert.That(previews.Count, Is.EqualTo(2));

            var movePreview = previews[0].ToDictionary();
            Assert.That(movePreview.ContainsKey(CellState.accessibleRoutePoint));
            Assert.That(movePreview[CellState.accessibleRoutePoint], Does.Contain(target));

            var hoverPreview = previews[1].ToDictionary();
            Assert.That(hoverPreview.ContainsKey(CellState.hovered));
            Assert.That(hoverPreview[CellState.hovered], Does.Contain(target));
        }

        [Test]
        public void HandleCellSelected_RaisesGridPreviewThroughSharedResolver()
        {
            SpawnActiveUnit(new Vector2Int(0, 0));

            var gridVm = new GridViewModel(_turnState, _resolver, _movement, _model);
            PreviewResult latestPreview = null;
            gridVm.PreviewChanged += preview => latestPreview = preview;

            var target = new Vector2Int(1, 0);
            var coords = new KeyValuePair<Vector2Int, Vector2Int>(target, target);

            latestPreview = null;
            _vm.HandleCellSelected(coords);

            Assert.That(latestPreview, Is.Not.Null);
            var dict = latestPreview.ToDictionary();
            Assert.That(dict.ContainsKey(CellState.accessibleRoutePoint));
            Assert.That(dict[CellState.accessibleRoutePoint], Does.Contain(target));
        }

        [Test]
        public void HandleCellSelected_ExecutesGameCommand()
        {
            SpawnActiveUnit(new Vector2Int(0, 0));

            var target = new Vector2Int(1, 0);
            var coords = new KeyValuePair<Vector2Int, Vector2Int>(target, target);

            _vm.HandleCellSelected(coords);

            Assert.That(_executor.LastType, Is.Not.Null);
            Assert.That(_executor.LastContext?.TargetCell, Is.EqualTo(target));
        }

        private UnitModel SpawnActiveUnit(Vector2Int position)
        {
            var spawnParams = new UnitSpawnParams(position.x, position.y, UnitType.Archer, 1, Team.Blue);
            var spawnResult = _model.SpawnUnit(spawnParams);
            Assert.That(spawnResult.IsSuccess, Is.True, "Unit spawn failed in test setup");

            var unit = (UnitModel)_model.GetCell(position).Unit;
            _turnService.AddCombatUnit(unit);
            _turnService.RunBattle();
            return unit;
        }

        private class FakeGameCommandExecutor : IGameCommandExecutor
        {
            public ActionType? LastType { get; private set; }
            public ActionContext? LastContext { get; private set; }

            public void Execute(ActionType type, ActionContext ctx)
            {
                LastType = type;
                LastContext = ctx;
            }

            public void StartBattle()
            {
            }
        }

        [TearDown]
        public void TearDown()
        {
            _turnState.Dispose();
        }
    }
}
