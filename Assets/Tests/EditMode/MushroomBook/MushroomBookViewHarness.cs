using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tests.EditMode.MushroomBook
{
    internal sealed class MushroomBookViewHarness
    {
        public MushroomBookView View { get; }

        private MushroomBookViewHarness(MushroomBookView view)
        {
            View = view;
        }

        public static MushroomBookViewHarness Create(int slotCount, IList<UnityEngine.Object> tracker)
        {
            var viewGo = new GameObject("MushroomBookView");
            tracker?.Add(viewGo);
            var view = viewGo.AddComponent<MushroomBookView>();

            var entries = new MushroomBookEntryViewHarness[slotCount];
            for (var i = 0; i < slotCount; i++)
            {
                entries[i] = MushroomBookEntryViewHarness.Create($"Slot_{i}", viewGo.transform, tracker);
            }

            var firstLabel = MushroomBookTestHelpers.CreateText(tracker, "FirstListNumberText", viewGo.transform);
            var secondLabel = MushroomBookTestHelpers.CreateText(tracker, "SecondListNumberText", viewGo.transform);

            var viewSO = new SerializedObject(view);
            var slotsProperty = MushroomBookTestHelpers.RequireProperty(viewSO, "pageSlots");
            slotsProperty.arraySize = slotCount;
            for (var i = 0; i < slotCount; i++)
            {
                slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = entries[i].View;
            }

            MushroomBookTestHelpers.RequireProperty(viewSO, "firstListNumberText").objectReferenceValue = firstLabel;
            MushroomBookTestHelpers.RequireProperty(viewSO, "secondListNumberText").objectReferenceValue = secondLabel;
            viewSO.ApplyModifiedProperties();

            return new MushroomBookViewHarness(view);
        }
    }
}
