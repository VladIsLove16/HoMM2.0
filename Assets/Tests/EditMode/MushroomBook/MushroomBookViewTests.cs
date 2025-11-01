using Adventure.Domain.Inventory;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using Game.Achievements;
using NUnit.Framework;
using UniRx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.EditMode.MushroomBook
{
    [TestFixture]
    public partial class MushroomBookViewTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new();
        private readonly List<IDisposable> _disposables = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }

            _disposables.Clear();

            foreach (var obj in _createdObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            _createdObjects.Clear();
        }

        [Test]
        public void Construct_WithViewModel_PopulatesSlotsAndPageNumber()
        {
            var harness = MushroomBookViewHarness.Create(2, _createdObjects);
            var viewModel = CreateViewModel(1, out var definitions);

            harness.View.Construct(viewModel);

            Assert.That(harness.View.PageCapacity, Is.EqualTo(2));
            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(definitions[0].DisplayName));
            Assert.That(harness.View.Slots[1].Title, Is.EqualTo(string.Empty));
            Assert.That(harness.View.CurrentPageLabel, Is.EqualTo("1"));
            Assert.That(harness.View.Slots[0].StatItemCount, Is.GreaterThan(0));
        }

        [Test]
        public void Construct_NullViewModel_ClearsSlotsAndResetsPageNumber()
        {
            var harness = MushroomBookViewHarness.Create(2, _createdObjects);
            var viewModel = CreateViewModel(1, out var definitions);

            harness.View.Construct(viewModel);
            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(definitions[0].DisplayName));

            harness.View.Construct(null);

            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(string.Empty));
            Assert.That(harness.View.Slots[1].Title, Is.EqualTo(string.Empty));
            Assert.That(harness.View.CurrentPageLabel, Is.EqualTo("0"));
        }

        [Test]
        public void Construct_RebindsWhenViewModelChanges()
        {
            var harness = MushroomBookViewHarness.Create(1, _createdObjects);
            var firstViewModel = CreateViewModel(1, out var firstDefinitions);
            var secondViewModel = CreateViewModel(1, out var secondDefinitions);

            harness.View.Construct(firstViewModel);
            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(firstDefinitions[0].DisplayName));

            harness.View.Construct(secondViewModel);

            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(secondDefinitions[0].DisplayName));
        }

        [Test]
        public void View_RespondsToIsOpenChanges()
        {
            var harness = MushroomBookViewHarness.Create(1, _createdObjects);
            harness.View.gameObject.SetActive(false);
            var viewModel = CreateViewModel(0, out _);

            harness.View.Construct(viewModel);

            viewModel.Open();
            Assert.That(harness.View.gameObject.activeSelf, Is.True);

            viewModel.Close();
            Assert.That(harness.View.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void PageNumberUpdates_WhenCurrentPageChanges()
        {
            var harness = MushroomBookViewHarness.Create(4, _createdObjects);
            var viewModel = CreateViewModel(5, out _);

            harness.View.Construct(viewModel);
            Assert.That(harness.View.CurrentPageLabel, Is.EqualTo("1"));

            viewModel.NextPage();

            Assert.That(harness.View.CurrentPageLabel, Is.EqualTo("2"));
        }

        private MushroomBookViewModel CreateViewModel(int entryCount, out MushroomBookEntryViewModel[] entries)
        {
            var defs = new List<UnitDefinitionSO>();
            var stacks = new List<UnitStackData>();

            for (int i = 0; i < entryCount; i++)
            {
                var unitType = (UnitType)(200 + i);
                var definition = MushroomBookTestHelpers.CreateDefinition(unitType, $"Entry {i}", _createdObjects, def =>
                {
                    def.Stats = MushroomBookTestHelpers.CreateStats(10 + i, 20 + i, 5 + i, 3 + i, _createdObjects);
                });
                defs.Add(definition);
                stacks.Add(new UnitStackData(unitType, 1));
            }

            var catalog = MushroomBookTestHelpers.CreateCatalog(defs, _createdObjects);
            var inventory = new MushroomInventoryModel(stacks);
            var viewModel = new MushroomBookViewModel(inventory, catalog, new StubAchievementEventBus());

            _disposables.Add(viewModel);
            entries = viewModel.CurrentPageEntries.ToArray();

            return viewModel;
        }
    }
}






