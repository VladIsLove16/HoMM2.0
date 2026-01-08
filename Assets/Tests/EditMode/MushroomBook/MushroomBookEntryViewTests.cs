using Adventure.Presentation.Mushroom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.MushroomBook
{

    [TestFixture]
    public class MushroomBookEntryViewTests
    {
        private readonly List<UnityEngine.Object> _createdObjects = new();

        [TearDown]
        public void TearDown()
        {
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
        public void Bind_SetsNameDescriptionAndStats()
        {
            var root = new GameObject("EntryRoot");
            _createdObjects.Add(root);
            var harness = MushroomBookEntryViewHarness.Create("EntryView", root.transform, _createdObjects);
            var stats = new List<MushroomStatViewData>
            {
                new MushroomStatViewData(UnitStatType.Health, "10"),
                new MushroomStatViewData(UnitStatType.MoveSpeed, "3")
            };
            var viewData = new MushroomBookEntryViewModel(UnitType.Witch, "Witch", "Swift attacker", null, null, null, null, stats, null);

            harness.View.Bind(viewData);

            Assert.That(harness.View.Title, Is.EqualTo("Witch"));
            Assert.That(harness.View.Description, Is.EqualTo("Swift attacker"));
            Assert.That(harness.View.StatItemCount, Is.EqualTo(2));
        }

        [Test]
        public void SetPresentationMode_UpdatesIcon()
        {
            var root = new GameObject("EntryRoot");
            _createdObjects.Add(root);
            var harness = MushroomBookEntryViewHarness.Create("EntryView", root.transform, _createdObjects);
            var icon = MushroomBookTestHelpers.CreateTestSprite(_createdObjects);
            var hovered = MushroomBookTestHelpers.CreateTestSprite(_createdObjects);
            var humanized = MushroomBookTestHelpers.CreateTestSprite(_createdObjects);
            var humanizedHovered = MushroomBookTestHelpers.CreateTestSprite(_createdObjects);
            icon.name = "IconSprite";
            hovered.name = "hovered";
            humanized.name = "humanized";
            humanizedHovered.name = "humanizedHovered";
            var viewData = new MushroomBookEntryViewModel(
                UnitType.Witch,
                "Witch",
                string.Empty,
                icon,
                hovered,
                humanized,
                humanizedHovered,
                Array.Empty<MushroomStatViewData>(), 
                null);

            harness.View.Bind(viewData);

            Assert.That(harness.View.CurrentSprite, Is.SameAs(icon));

            harness.View.SetPresentationMode(PresentationMode.Normal);
            Assert.That(harness.View.CurrentSprite, Is.SameAs(icon));

            harness.View.SetPresentationMode(PresentationMode.Humanized);
            Assert.That(harness.View.CurrentSprite, Is.SameAs(humanized));
        }

        [Test]
        public void Bind_WithNullData_ResetsToEmptyState()
        {
            var root = new GameObject("EntryRoot");
            _createdObjects.Add(root);
            var harness = MushroomBookEntryViewHarness.Create("EntryView", root.transform, _createdObjects);

            harness.View.Bind(null);

            Assert.That(harness.View.Title, Is.EqualTo(string.Empty));
            Assert.That(harness.View.Description, Is.EqualTo(string.Empty));
            Assert.That(harness.View.StatItemCount, Is.EqualTo(0));
        }

        [Test]
        public void Bind_SetsAmountText()
        {
            var root = new GameObject("EntryRoot");
            _createdObjects.Add(root);
            var harness = MushroomBookEntryViewHarness.Create("EntryView", root.transform, _createdObjects);
            var viewData = new MushroomBookEntryViewModel(UnitType.Witch, "Witch", "Swift attacker", null, null, null, null, Array.Empty<MushroomStatViewData>(), null);
            viewData.Amount = 3;

            harness.View.Bind(viewData);

            Assert.That(harness.View.AmountText, Is.EqualTo("3"));
        }
    }
}
