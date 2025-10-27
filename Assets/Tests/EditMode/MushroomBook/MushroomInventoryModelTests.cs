using Adventure.Domain.Inventory;
using Adventure.Integration.Battle;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests.EditMode.MushroomBook
{
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
}
