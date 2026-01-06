using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Inventory;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
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
            var viewModel = CreateViewModel(1, out var entryNames);

            harness.View.Construct(viewModel);

            Assert.That(harness.View.PageCapacity, Is.EqualTo(2));
            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(entryNames[0]));
            Assert.That(harness.View.Slots[1].Title, Is.EqualTo(string.Empty));
            Assert.That(harness.View.CurrentPageLabel, Is.EqualTo("1"));
            Assert.That(harness.View.Slots[0].StatItemCount, Is.GreaterThan(0));
        }

        [Test]
        public void Construct_NullViewModel_ClearsSlotsAndResetsPageNumber()
        {
            var harness = MushroomBookViewHarness.Create(2, _createdObjects);
            var viewModel = CreateViewModel(1, out var entryNames);

            harness.View.Construct(viewModel);
            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(entryNames[0]));

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
            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(firstDefinitions[0]));

            harness.View.Construct(secondViewModel);

            Assert.That(harness.View.Slots[0].Title, Is.EqualTo(secondDefinitions[0]));
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

        private MushroomBookViewModel CreateViewModel(int entryCount, out string[] entryNames)
        {
            var names = new List<string>();
            var stacks = new List<UnitStackData>();
            var entries = new List<(UnitType type, string displayName, UnitStatsInline stats)>();

            for (int i = 0; i < entryCount; i++)
            {
                var unitType = (UnitType)(200 + i);
                var name = $"Entry {i}";
                names.Add(name);
                stacks.Add(new UnitStackData(unitType, 1));
                entries.Add((unitType, name, new UnitStatsInline
                {
                    Health = 10 + i,
                    MaxHealth = 20 + i,
                    Damage = 5 + i,
                    MoveSpeed = 3 + i
                }));
            }

            var catalog = TestDataFactory.CreateAdventureMushroomMap(_createdObjects, entries.ToArray());
            var inventory = new MushroomInventoryModel(stacks);
            var viewModel = new MushroomBookViewModel(inventory, catalog, new StubAchievementEventBus());

            _disposables.Add(viewModel);
            entryNames = names.ToArray();

            return viewModel;
        }
    }
}







