using Adventure.Presentation.Mushroom;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditMode.MushroomBook
{
    internal sealed class MushroomBookEntryViewHarness
    {
        public MushroomBookEntryView View { get; }
        public MushroomImageController ImageController { get; }
        public Image Picture { get; }
        public TextMeshProUGUI NameText { get; }
        public TextMeshProUGUI DescriptionText { get; }
        public Transform StatsRoot { get; }
        public UnitSingleStatPanel StatItemPrefab { get; }
        public StatIcons StatIcons { get; }

        private MushroomBookEntryViewHarness(
            MushroomBookEntryView view,
            MushroomImageController imageController,
            Image picture,
            TextMeshProUGUI nameText,
            TextMeshProUGUI descriptionText,
            Transform statsRoot,
            UnitSingleStatPanel statItemPrefab,
            StatIcons statIcons)
        {
            View = view;
            ImageController = imageController;
            Picture = picture;
            NameText = nameText;
            DescriptionText = descriptionText;
            StatsRoot = statsRoot;
            StatItemPrefab = statItemPrefab;
            StatIcons = statIcons;
        }

        public static MushroomBookEntryViewHarness Create(string name, Transform parent, IList<UnityEngine.Object> tracker)
        {
            var go = new GameObject(name);
            tracker?.Add(go);
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<MushroomBookEntryView>();

            var imageGo = new GameObject($"{name}_Image");
            tracker?.Add(imageGo);
            imageGo.transform.SetParent(go.transform, false);
            var picture = imageGo.AddComponent<Image>();
            var controller = imageGo.AddComponent<MushroomImageController>();

            var controllerSO = new SerializedObject(controller);
            MushroomBookTestHelpers.RequireProperty(controllerSO, "picture").objectReferenceValue = picture;
            controllerSO.ApplyModifiedProperties();

            var nameText = MushroomBookTestHelpers.CreateText(tracker, $"{name}_Name", go.transform);
            var descriptionText = MushroomBookTestHelpers.CreateText(tracker, $"{name}_Description", go.transform);
            var statsRoot = MushroomBookTestHelpers.CreateContainer(tracker, $"{name}_StatsRoot", go.transform);

            var statPrefab = new GameObject($"{name}_StatPrefab");
            tracker?.Add(statPrefab);
            statPrefab.AddComponent<RectTransform>();
            var statPanel = statPrefab.AddComponent<UnitSingleStatPanel>();
            var statIcon = statPrefab.AddComponent<Image>();
            var statLabel = MushroomBookTestHelpers.CreateText(tracker, $"{name}_StatLabel", statPrefab.transform);
            var statAmount = MushroomBookTestHelpers.CreateText(tracker, $"{name}_StatAmount", statPrefab.transform);

            var statPanelSO = new SerializedObject(statPanel);
            MushroomBookTestHelpers.RequireProperty(statPanelSO, "Image").objectReferenceValue = statIcon;
            MushroomBookTestHelpers.RequireProperty(statPanelSO, "StatText").objectReferenceValue = statLabel;
            MushroomBookTestHelpers.RequireProperty(statPanelSO, "StatAmount").objectReferenceValue = statAmount;
            statPanelSO.ApplyModifiedProperties();

            var statIcons = ScriptableObject.CreateInstance<StatIcons>();
            tracker?.Add(statIcons);
            statIcons.statIconBindings.Add(new StatIconBinding
            {
                Type = UnitStatType.Health,
                Icon = MushroomBookTestHelpers.CreateTestSprite(tracker)
            });

            var viewSO = new SerializedObject(view);
            MushroomBookTestHelpers.RequireProperty(viewSO, "mushroomImageController").objectReferenceValue = controller;
            MushroomBookTestHelpers.RequireProperty(viewSO, "nameText").objectReferenceValue = nameText;
            MushroomBookTestHelpers.RequireProperty(viewSO, "descriptionText").objectReferenceValue = descriptionText;
            MushroomBookTestHelpers.RequireProperty(viewSO, "statsRoot").objectReferenceValue = statsRoot;
            MushroomBookTestHelpers.RequireProperty(viewSO, "statItemPrefab").objectReferenceValue = statPanel;
            MushroomBookTestHelpers.RequireProperty(viewSO, "statIcons").objectReferenceValue = statIcons;
            viewSO.ApplyModifiedProperties();

            return new MushroomBookEntryViewHarness(view, controller, picture, nameText, descriptionText, statsRoot, statPanel, statIcons);
        }
    }
}
