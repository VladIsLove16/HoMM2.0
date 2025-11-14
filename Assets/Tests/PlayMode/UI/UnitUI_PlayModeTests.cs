using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using Tests.Common;

namespace Tests.PlayMode.UI
{
    [TestFixture]
    public class UnitUI_PlayModeTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly List<ScriptableObject> _assets = new();

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var go in _objects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            _objects.Clear();
            foreach (var asset in _assets)
            {
                if (asset != null)
                {
                    Object.DestroyImmediate(asset);
                }
            }
            _assets.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Init_PopulatesHealthAndAmountTexts()
        {
            var fixture = CreateFixture(amount: 3);
            fixture.UnitViewUI.Init(fixture.ViewModel);
            yield return null;

            Assert.That(fixture.AmountText.text, Is.EqualTo("3"));
            Assert.That(fixture.HealthAmountText.text, Is.EqualTo("100/100"));
        }

        [UnityTest]
        public IEnumerator RecieveDamage_UpdatesHealthTextsAndFill()
        {
            var fixture = CreateFixture();
            fixture.UnitViewUI.Init(fixture.ViewModel);
            yield return null;

            fixture.Model.RecieveDamage(new DamageContext(30, DamageType.physical, null));
            yield return null;

            Assert.That(fixture.HealthAmountText.text, Is.EqualTo("70/100"));
            Assert.That(fixture.HealthBarImage.fillAmount, Is.EqualTo(0.7f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator LethalDamage_ChangesAmountColor()
        {
            var fixture = CreateFixture(amount: 2);
            fixture.UnitViewUI.Init(fixture.ViewModel);
            yield return null;

            fixture.Model.RecieveDamage(new DamageContext(200, DamageType.physical, null));
            yield return null;

            Assert.That(fixture.AmountText.color, Is.EqualTo(Color.black));
        }

        [UnityTest]
        public IEnumerator Dispose_PreventsFurtherUpdates()
        {
            var fixture = CreateFixture();
            fixture.UnitViewUI.Init(fixture.ViewModel);
            yield return null;

            var initialText = fixture.HealthAmountText.text;
            fixture.UnitViewUI.Dispose();

            fixture.Model.RecieveDamage(new DamageContext(40, DamageType.physical, null));
            yield return null;

            Assert.That(fixture.HealthAmountText.text, Is.EqualTo(initialText));
        }

        [UnityTest]
        public IEnumerator MultipleDamageEvents_AccumulateCorrectly()
        {
            var fixture = CreateFixture(amount: 3);
            fixture.UnitViewUI.Init(fixture.ViewModel);
            yield return null;

            fixture.Model.RecieveDamage(new DamageContext(40, DamageType.physical, null));
            yield return null;
            fixture.Model.RecieveDamage(new DamageContext(70, DamageType.physical, null));
            yield return null;

            Assert.That(fixture.HealthAmountText.text, Is.EqualTo("90/100"));
            Assert.That(fixture.AmountText.text, Is.EqualTo("2"));
        }

        private UITestFixture CreateFixture(int amount = 1)
        {
            var cameraGO = new GameObject("PlayModeCamera");
            cameraGO.tag = "MainCamera";
            cameraGO.AddComponent<Camera>();
            _objects.Add(cameraGO);

            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            _objects.Add(canvasGO);

            var healthBarGO = new GameObject("HealthBar");
            healthBarGO.transform.SetParent(canvasGO.transform);
            var healthBar = healthBarGO.AddComponent<UnitHealthBar>();
            var healthImage = healthBarGO.AddComponent<Image>();
            healthImage.type = Image.Type.Filled;
            healthImage.fillMethod = Image.FillMethod.Horizontal;

            var healthTextGO = new GameObject("HealthText");
            healthTextGO.transform.SetParent(healthBarGO.transform);
            var healthText = healthTextGO.AddComponent<TextMeshProUGUI>();

            typeof(UnitHealthBar).GetField("HealthText", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(healthBar, healthText);
            typeof(UnitHealthBar).GetField("HealthImage", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(healthBar, healthImage);

            var amountTextGO = new GameObject("AmountText");
            amountTextGO.transform.SetParent(canvasGO.transform);
            var amountText = amountTextGO.AddComponent<TextMeshProUGUI>();

            var healthAmountGO = new GameObject("HealthAmountText");
            healthAmountGO.transform.SetParent(canvasGO.transform);
            var healthAmountText = healthAmountGO.AddComponent<TextMeshProUGUI>();

            var uiGO = new GameObject("UnitViewUI");
            uiGO.transform.SetParent(canvasGO.transform);
            var unitViewUI = uiGO.AddComponent<UnitViewUI>();

            typeof(UnitViewUI).GetField("healthBar", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(unitViewUI, healthBar);
            typeof(UnitViewUI).GetField("amountText", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(unitViewUI, amountText);
            typeof(UnitViewUI).GetField("healthAmountText", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(unitViewUI, healthAmountText);

            _objects.Add(healthBarGO);
            _objects.Add(amountTextGO);
            _objects.Add(healthAmountGO);
            _objects.Add(uiGO);

            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.Health = 100;
            stats.MaxHealth = 100;
            stats.Damage = 10;
            stats.MoveSpeed = 3;
            stats.InvulnerableEffects = new List<StatusEffectType>();
            _assets.Add(stats);

            var model = new UnitModel(stats, UnitType.Archer, 0, 0, amount, Team.Blue);
            var viewModel = new UnitViewModel(model, new TestWorldToCellProvider());

            return new UITestFixture(unitViewUI, viewModel, model, amountText, healthAmountText, healthImage);
        }

        private class UITestFixture
        {
            public UITestFixture(UnitViewUI unitViewUI, UnitViewModel viewModel, UnitModel model, TextMeshProUGUI amountText, TextMeshProUGUI healthAmountText, Image healthBarImage)
            {
                UnitViewUI = unitViewUI;
                ViewModel = viewModel;
                Model = model;
                AmountText = amountText;
                HealthAmountText = healthAmountText;
                HealthBarImage = healthBarImage;
            }

            public UnitViewUI UnitViewUI { get; }
            public UnitViewModel ViewModel { get; }
            public UnitModel Model { get; }
            public TextMeshProUGUI AmountText { get; }
            public TextMeshProUGUI HealthAmountText { get; }
            public Image HealthBarImage { get; }
        }
    }
}


