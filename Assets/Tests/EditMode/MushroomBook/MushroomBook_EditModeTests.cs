using Adventure.Domain.Inventory;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Tests.EditMode.MushroomBook
{
    internal static class MushroomBookTestHelpers
    {
        private static readonly FieldInfo UnitCollectionField =
            typeof(UnitDefinitionSOCollection).GetField("unitDefinitionSOs", BindingFlags.NonPublic | BindingFlags.Instance);

        public static UnitDefinitionSO CreateDefinition(UnitType type, string name, IList<UnityEngine.Object> tracker)
        {
            var definition = ScriptableObject.CreateInstance<UnitDefinitionSO>();
            definition.UnitType = type;
            definition.Name = name;
            tracker?.Add(definition);
            return definition;
        }

        public static UnitDefinitionSOCollection CreateCatalog(IEnumerable<UnitDefinitionSO> definitions, IList<UnityEngine.Object> tracker)
        {
            if (UnitCollectionField == null)
                throw new InvalidOperationException("unitDefinitionSOs field not found on UnitDefinitionSOCollection.");

            var catalog = ScriptableObject.CreateInstance<UnitDefinitionSOCollection>();
            UnitCollectionField.SetValue(catalog, definitions.ToList());
            tracker?.Add(catalog);
            return catalog;
        }
    }

    [TestFixture]
    public class MushroomInventoryModelTests
    {
        [Test]
        public void Constructor_PopulatesItemsFromStacks()
        {
            var stacks = new List<UnitStackData>
            {
                new UnitStackData(UnitType.Witch, 2),
                new UnitStackData(UnitType.Warrok, 5)
            };

            var model = new MushroomInventoryModel(stacks);

            Assert.That(model.Items[UnitType.Witch], Is.EqualTo(2));
            Assert.That(model.Items[UnitType.Warrok], Is.EqualTo(5));
        }

        [Test]
        public void Add_IncrementsExistingAmount()
        {
            var model = new MushroomInventoryModel(new List<UnitStackData>());

            model.Add(UnitType.Witch);
            model.Add(UnitType.Witch, 2);

            Assert.That(model.GetCount(UnitType.Witch), Is.EqualTo(3));
        }

        [Test]
        public void GetData_ReturnsCurrentItemsSnapshot()
        {
            var model = new MushroomInventoryModel(new List<UnitStackData>());
            model.Add(UnitType.Witch, 4);
            model.Add(UnitType.Warrok, 1);

            var data = model.GetData();

            CollectionAssert.AreEquivalent(
                new[] { new UnitStackData(UnitType.Witch, 4), new UnitStackData(UnitType.Warrok, 1) },
                data);
        }
    }

    [TestFixture]
    public class MushroomBookViewModelTests
    {
        private readonly List<UnityEngine.Object> _createdAssets = new List<UnityEngine.Object>();

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
        public void Constructor_WhenInventoryHasMoreThanPage_SplitsIntoMultiplePages()
        {
            var viewModel = CreateViewModel(5, out _, out _, out var definitions);

            try
            {
                Assert.That(viewModel.TotalPages.Value, Is.EqualTo(2));
                Assert.That(viewModel.CurrentPage.Value, Is.EqualTo(0));
                Assert.That(viewModel.CurrentPageEntries.Count, Is.EqualTo(4));
                CollectionAssert.AreEquivalent(definitions.Take(4).ToArray(), viewModel.CurrentPageEntries.ToArray());
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void NextAndPrevPage_StayWithinValidRange()
        {
            var viewModel = CreateViewModel(5, out _, out _, out var definitions);

            try
            {
                viewModel.NextPage();
                Assert.That(viewModel.CurrentPage.Value, Is.EqualTo(1));
                Assert.That(viewModel.CurrentPageEntries.Count, Is.EqualTo(1));
                Assert.That(viewModel.CurrentPageEntries[0], Is.EqualTo(definitions.Last()));

                viewModel.NextPage();
                Assert.That(viewModel.CurrentPage.Value, Is.EqualTo(1));

                viewModel.PrevPage();
                Assert.That(viewModel.CurrentPage.Value, Is.EqualTo(0));

                viewModel.GoToPage(-10);
                Assert.That(viewModel.CurrentPage.Value, Is.EqualTo(0));
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void OpenCloseToggle_UpdatesIsOpenProperty()
        {
            var viewModel = CreateViewModel(1, out _, out _, out _);

            try
            {
                Assert.That(viewModel.IsOpen.Value, Is.False);

                viewModel.Open();
                Assert.That(viewModel.IsOpen.Value, Is.True);

                viewModel.Open();
                Assert.That(viewModel.IsOpen.Value, Is.True);

                viewModel.Close();
                Assert.That(viewModel.IsOpen.Value, Is.False);

                viewModel.Toggle();
                Assert.That(viewModel.IsOpen.Value, Is.True);

                viewModel.Toggle();
                Assert.That(viewModel.IsOpen.Value, Is.False);
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void Collect_AddsNewItemToInventory()
        {
            var definition = MushroomBookTestHelpers.CreateDefinition(UnitType.Witch, "Witch Mushroom", _createdAssets);
            var catalog = MushroomBookTestHelpers.CreateCatalog(new[] { definition }, _createdAssets);
            var inventory = new MushroomInventoryModel(new List<UnitStackData>());
            var viewModel = new MushroomBookViewModel(inventory, catalog);

            try
            {
                Assert.That(inventory.GetCount(UnitType.Witch), Is.EqualTo(0));

                viewModel.Collect(UnitType.Witch);

                Assert.That(inventory.GetCount(UnitType.Witch), Is.EqualTo(1));
            }
            finally
            {
                viewModel.Dispose();
            }
        }

        [Test]
        public void SetPresentationMode_UpdatesReactiveProperty()
        {
            var viewModel = CreateViewModel(1, out _, out _, out _);

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
            out UnitDefinitionSO[] definitions)
        {
            var defs = new List<UnitDefinitionSO>();
            var stacks = new List<UnitStackData>();

            for (int i = 0; i < entryCount; i++)
            {
                var unitType = (UnitType)(100 + i);
                var definition = MushroomBookTestHelpers.CreateDefinition(unitType, $"Mushroom #{i}", _createdAssets);
                defs.Add(definition);
                stacks.Add(new UnitStackData(unitType, 1));
            }

            inventory = new MushroomInventoryModel(stacks);
            catalog = MushroomBookTestHelpers.CreateCatalog(defs, _createdAssets);
            definitions = defs.ToArray();

            return new MushroomBookViewModel(inventory, catalog);
        }
    }

    [TestFixture]
    public class MushroomBookViewTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new List<UnityEngine.Object>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

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
            var view = CreateView(2, out _, out var nameTexts, out var pageLabel);
            var viewModel = CreateViewModel(1, out var definitions);

            view.Construct(viewModel);

            Assert.That(view.PageCapacity, Is.EqualTo(2));
            Assert.That(nameTexts[0].text, Is.EqualTo(definitions[0].Name));
            Assert.That(nameTexts[1].text, Is.EqualTo(string.Empty));
            Assert.That(pageLabel.text, Is.EqualTo("1"));
        }

        [Test]
        public void Construct_NullViewModel_ClearsSlotsAndResetsPageNumber()
        {
            var view = CreateView(2, out _, out var nameTexts, out var pageLabel);
            var viewModel = CreateViewModel(1, out var definitions);

            view.Construct(viewModel);
            Assert.That(nameTexts[0].text, Is.EqualTo(definitions[0].Name));

            view.Construct(null);

            Assert.That(nameTexts[0].text, Is.EqualTo(string.Empty));
            Assert.That(nameTexts[1].text, Is.EqualTo(string.Empty));
            Assert.That(pageLabel.text, Is.EqualTo("0"));
        }

        [Test]
        public void Construct_RebindsWhenViewModelChanges()
        {
            var view = CreateView(1, out _, out var nameTexts, out _);
            var firstViewModel = CreateViewModel(1, out var firstDefinitions);
            var secondViewModel = CreateViewModel(1, out var secondDefinitions);

            view.Construct(firstViewModel);
            Assert.That(nameTexts[0].text, Is.EqualTo(firstDefinitions[0].Name));

            view.Construct(secondViewModel);

            Assert.That(nameTexts[0].text, Is.EqualTo(secondDefinitions[0].Name));
        }

        [Test]
        public void View_RespondsToIsOpenChanges()
        {
            var view = CreateView(1, out _, out _, out _);
            view.gameObject.SetActive(false);
            var viewModel = CreateViewModel(0, out _);

            view.Construct(viewModel);

            viewModel.Open();
            Assert.That(view.gameObject.activeSelf, Is.True);

            viewModel.Close();
            Assert.That(view.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void PageNumberUpdates_WhenCurrentPageChanges()
        {
            var view = CreateView(4, out _, out _, out var pageLabel);
            var viewModel = CreateViewModel(5, out _);

            view.Construct(viewModel);
            Assert.That(pageLabel.text, Is.EqualTo("1"));

            viewModel.NextPage();

            Assert.That(pageLabel.text, Is.EqualTo("2"));
        }

        private MushroomBookView CreateView(
            int slotCount,
            out MushroomBookEntryView[] slots,
            out TextMeshProUGUI[] nameTexts,
            out TextMeshProUGUI pageLabel)
        {
            var viewGo = new GameObject("MushroomBookView");
            _createdObjects.Add(viewGo);
            var view = viewGo.AddComponent<MushroomBookView>();

            var slotList = new List<MushroomBookEntryView>();
            var nameList = new List<TextMeshProUGUI>();

            for (int i = 0; i < slotCount; i++)
            {
                var slotGo = new GameObject($"Slot_{i}");
                _createdObjects.Add(slotGo);
                slotGo.transform.SetParent(viewGo.transform, false);
                var slot = slotGo.AddComponent<MushroomBookEntryView>();
                slotList.Add(slot);

                var name = new GameObject($"Name_{i}").AddComponent<TextMeshProUGUI>();
                _createdObjects.Add(name.gameObject);
                name.transform.SetParent(slotGo.transform, false);
                SetPrivateField(slot, "nameText", name);
                nameList.Add(name);

                var description = new GameObject($"Description_{i}").AddComponent<TextMeshProUGUI>();
                _createdObjects.Add(description.gameObject);
                description.transform.SetParent(slotGo.transform, false);
                SetPrivateField(slot, "descriptionText", description);
            }

            pageLabel = new GameObject("PageLabel").AddComponent<TextMeshProUGUI>();
            _createdObjects.Add(pageLabel.gameObject);
            pageLabel.transform.SetParent(viewGo.transform, false);

            SetPrivateField(view, "entrySlots", slotList);
            SetPrivateField(view, "pageNumberText", pageLabel);

            slots = slotList.ToArray();
            nameTexts = nameList.ToArray();

            return view;
        }

        private MushroomBookViewModel CreateViewModel(int entryCount, out UnitDefinitionSO[] definitions)
        {
            var defs = new List<UnitDefinitionSO>();
            var stacks = new List<UnitStackData>();

            for (int i = 0; i < entryCount; i++)
            {
                var unitType = (UnitType)(200 + i);
                var definition = MushroomBookTestHelpers.CreateDefinition(unitType, $"Entry {i}", _createdObjects);
                defs.Add(definition);
                stacks.Add(new UnitStackData(unitType, 1));
            }

            var catalog = MushroomBookTestHelpers.CreateCatalog(defs, _createdObjects);
            var inventory = new MushroomInventoryModel(stacks);
            var viewModel = new MushroomBookViewModel(inventory, catalog);

            _disposables.Add(viewModel);
            definitions = defs.ToArray();

            return viewModel;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} field was not found.");
            field.SetValue(target, value);
        }
    }
}
