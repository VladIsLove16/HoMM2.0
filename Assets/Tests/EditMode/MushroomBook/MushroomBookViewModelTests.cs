using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Inventory;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using Game.Achievements;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Tests.EditMode.MushroomBook
{
    [TestFixture]
    public partial class MushroomBookViewModelTests
    {
        private readonly List<UnityEngine.Object> _createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in _createdAssets)
            {
                if (asset != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            _createdAssets.Clear();
        }

        [Test]
        public void Collect_AddsNewItemToInventory()
        {
            var catalog = TestDataFactory.CreateSingleAdventureMushroom(_createdAssets, UnitType.Witch, moveSpeed: 3, health: 10);
            var inventory = new MushroomInventoryModel(new List<UnitStackData>());
            var bus = new StubAchievementEventBus();
            var viewModel = new MushroomBookViewModel(inventory, catalog, bus);

            try
            {
                Assert.That(inventory.GetAmount(UnitType.Witch), Is.EqualTo(0));

                viewModel.Collect(UnitType.Witch);

                Assert.That(inventory.GetAmount(UnitType.Witch), Is.EqualTo(1));
                Assert.That(bus.Collected.Count, Is.EqualTo(1));
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void Constructor_PopulatesInitialPage()
        {
            var viewModel = CreateViewModel(3, out _, out _, out var displayNames, out _);

            try
            {
                Assert.That(viewModel.CurrentPageEntries.Count, Is.EqualTo(3));
                Assert.That(viewModel.CurrentPageEntries[0].DisplayName, Is.EqualTo(displayNames[0]));
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void NextPage_WhenMultiplePages_UpdatesCurrentPage()
        {
            var viewModel = CreateViewModel(4, out _, out _, out _, out _);

            try
            {
                viewModel.NextPage();

                Assert.That(viewModel.CurrentPage.Value, Is.EqualTo(1));
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void PreviousPage_WhenOnFirstPage_DoesNotGoNegative()
        {
            var viewModel = CreateViewModel(4, out _, out _, out _, out _);
            try
            {
                viewModel.PrevPage();

                Assert.That(viewModel.CurrentPage.Value, Is.EqualTo(0));
            }
            finally
            {
                viewModel.Dispose();
            }
        }
        [Test]
        public void SetPresentationMode_UpdatesReactiveProperty()
        {
            var viewModel = CreateViewModel(1, out _, out _, out _, out _);

            try
            {
                Assert.That(viewModel.PresentationMode.Value, Is.EqualTo(PresentationMode.Normal));

                viewModel.SetPresentationMode(PresentationMode.Humanized);
                Assert.That(viewModel.PresentationMode.Value, Is.EqualTo(PresentationMode.Humanized));

                viewModel.SetPresentationMode(PresentationMode.Humanized);
                Assert.That(viewModel.PresentationMode.Value, Is.EqualTo(PresentationMode.Humanized));
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void Collect_RefreshesCurrentPageEntriesAndAmount()
        {
            var catalog = TestDataFactory.CreateSingleAdventureMushroom(_createdAssets, UnitType.Witch, moveSpeed: 3, health: 10);
            var inventory = new MushroomInventoryModel(new List<UnitStackData>());
            var bus = new StubAchievementEventBus();
            var viewModel = new MushroomBookViewModel(inventory, catalog, bus);

            try
            {
                Assert.That(viewModel.CurrentPageEntries.Count, Is.EqualTo(0));

                viewModel.Collect(UnitType.Witch);
                Assert.That(viewModel.CurrentPageEntries.Count, Is.EqualTo(1));
                Assert.That(viewModel.CurrentPageEntries[0].Amount, Is.EqualTo(1));

                viewModel.Collect(UnitType.Witch);
                Assert.That(viewModel.CurrentPageEntries.Count, Is.EqualTo(1));
                Assert.That(viewModel.CurrentPageEntries[0].Amount, Is.EqualTo(2));
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        private MushroomBookViewModel CreateViewModel(
            int entryCount,
            out MushroomInventoryModel inventory,
            out AdventureMushroomAssetMap catalog,
            out string[] displayNames,
            out StubAchievementEventBus bus)
        {
            var names = new List<string>();
            var stacks = new List<UnitStackData>();
            var entries = new List<(UnitType type, string displayName, UnitStatsInline stats)>();

            for (int i = 0; i < entryCount; i++)
            {
                var unitType = (UnitType)(100 + i);
                var name = $"Mushroom #{i}";
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

            inventory = new MushroomInventoryModel(stacks);
            catalog = TestDataFactory.CreateAdventureMushroomMap(
                _createdAssets,
                entries.ToArray());
            displayNames = names.ToArray();

            bus = new StubAchievementEventBus();
            return new MushroomBookViewModel(inventory, catalog, bus);
        }
    }
}












