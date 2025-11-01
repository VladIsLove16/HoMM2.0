using Adventure.Domain.Inventory;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using Game.Achievements;
using NUnit.Framework;
using System.Collections.Generic;

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
            var definition = MushroomBookTestHelpers.CreateDefinition(UnitType.Witch, "Witch Mushroom", _createdAssets, def =>
            {
                def.Stats = MushroomBookTestHelpers.CreateStats(10, 20, 5, 3, _createdAssets);
            });
            var catalog = MushroomBookTestHelpers.CreateCatalog(new[] { definition }, _createdAssets);
            var inventory = new MushroomInventoryModel(new List<UnitStackData>());
            var bus = new StubAchievementEventBus();
            var viewModel = new MushroomBookViewModel(inventory, catalog, bus);

            try
            {
                Assert.That(inventory.GetCount(UnitType.Witch), Is.EqualTo(0));

                viewModel.Collect(UnitType.Witch);

                Assert.That(inventory.GetCount(UnitType.Witch), Is.EqualTo(1));
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
            var viewModel = CreateViewModel(3, out _, out _, out var definitions, out _);

            try
            {
                Assert.That(viewModel.CurrentPageEntries.Count, Is.EqualTo(3));
                Assert.That(viewModel.CurrentPageEntries[0].DisplayName, Is.EqualTo(definitions[0].DisplayName));
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

        private MushroomBookViewModel CreateViewModel(
            int entryCount,
            out MushroomInventoryModel inventory,
            out UnitDefinitionSOCollection catalog,
            out UnitDefinitionSO[] definitions,
            out StubAchievementEventBus bus)
        {
            var defs = new List<UnitDefinitionSO>();
            var stacks = new List<UnitStackData>();

            for (int i = 0; i < entryCount; i++)
            {
                var unitType = (UnitType)(100 + i);
                var definition = MushroomBookTestHelpers.CreateDefinition(unitType, $"Mushroom #{i}", _createdAssets, def =>
                {
                    def.Stats = MushroomBookTestHelpers.CreateStats(10 + i, 20 + i, 5 + i, 3 + i, _createdAssets);
                });
                defs.Add(definition);
                stacks.Add(new UnitStackData(unitType, 1));
            }

            inventory = new MushroomInventoryModel(stacks);
            catalog = MushroomBookTestHelpers.CreateCatalog(defs, _createdAssets);
            definitions = defs.ToArray();

            bus = new StubAchievementEventBus();
            return new MushroomBookViewModel(inventory,catalog,bus);
        }
    }
}











