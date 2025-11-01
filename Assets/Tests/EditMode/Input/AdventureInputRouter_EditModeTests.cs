using System;
using System.Collections.Generic;
using System.Reflection;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using Game.Achievements;
using Adventure.Infrastructure.Events;
using UniRx;
using Adventure.Settings.Model;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests.EditMode.Input
{
    [TestFixture]
    public class AdventureInputRouter_EditModeTests
    {
        private AdventureInput _input;
        private InputModeStub _inputMode;
        private TestActiveMenu _menu;
        private MenusCoordinatorViewModel _menus;
        private GameSettingsViewModel _settings;
        private MushroomBookViewModel _book;
        private UnitDefinitionSOCollection _catalog;
        private PlayerMovementController _movementController;
        private PlayerInteractionController _interactionController;
        private MovementSettingsSO _movementSettings;
        private GameObject _movementGO;
        private GameObject _interactionGO;

        [SetUp]
        public void SetUp()
        {
            _input = new AdventureInput();
            _inputMode = new InputModeStub();
            _menu = new TestActiveMenu(InputMode.Blocked);
            _menus = new MenusCoordinatorViewModel(_inputMode, new[] { _menu });
            _settings = new GameSettingsViewModel(new GameSettingsModel(), new PauseController(), new AnimationSpeedSettings());
            _catalog = ScriptableObject.CreateInstance<UnitDefinitionSOCollection>();
            _book = new MushroomBookViewModel(new MushroomInventoryModel(new List<UnitStackData>()), _catalog, new StubAchievementEventBus());
            _movementController = CreateMovementController(out _movementGO);
            _interactionController = CreateInteractionController(out _interactionGO, _input);
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_movementGO != null) Object.DestroyImmediate(_movementGO);
            if (_interactionGO != null) Object.DestroyImmediate(_interactionGO);
            if (_movementSettings != null) Object.DestroyImmediate(_movementSettings);
            if (_catalog != null) Object.DestroyImmediate(_catalog);
            _book?.Dispose();
            Time.timeScale = 1f;
        }

        [Test]
        public void MoveChanged_WhenMovementAllowed_UpdatesMovementInput()
        {
            var router = CreateRouter();
            try
            {
                var expected = new Vector2(0.5f, -1f);

                RaiseMove(_input, expected);

                Assert.That(GetMovementField<Vector2>("_moveInput"), Is.EqualTo(expected));
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void MoveChanged_WhenMovementBlocked_DoesNotUpdate()
        {
            _inputMode.CanMove = false;
            var router = CreateRouter();
            try
            {
                RaiseMove(_input, new Vector2(1f, 0f));

                Assert.That(GetMovementField<Vector2>("_moveInput"), Is.EqualTo(Vector2.zero));
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void LookChanged_WhenLookAllowed_AccumulatesDelta()
        {
            var router = CreateRouter();
            try
            {
                var delta = new Vector2(-2f, 4f);

                RaiseLook(_input, delta);

                Assert.That(GetMovementField<Vector2>("_pendingLookInput"), Is.EqualTo(delta));
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void LookChanged_WhenLookBlocked_DoesNothing()
        {
            _inputMode.CanLook = false;
            var router = CreateRouter();
            try
            {
                RaiseLook(_input, new Vector2(3f, 1f));

                Assert.That(GetMovementField<Vector2>("_pendingLookInput"), Is.EqualTo(Vector2.zero));
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void SprintChanged_WhenMovementAllowed_UpdatesSprintFlag()
        {
            var router = CreateRouter();
            try
            {
                RaiseSprint(_input, true);

                Assert.That(GetMovementField<bool>("_sprintInput"), Is.True);
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void SprintChanged_WhenMovementBlocked_DoesNothing()
        {
            _inputMode.CanMove = false;
            var router = CreateRouter();
            try
            {
                RaiseSprint(_input, true);

                Assert.That(GetMovementField<bool>("_sprintInput"), Is.False);
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void Interact_WhenMovementAllowed_InvokesController()
        {
            var router = CreateRouter();
            try
            {
                LogAssert.Expect(LogType.Log, "interaction performed");
                LogAssert.Expect(LogType.Warning, "PlayerInteractionController not enabled!");

                RaiseInteract(_input);
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void Interact_WhenMovementBlocked_SuppressesInteraction()
        {
            _inputMode.CanMove = false;
            var router = CreateRouter();
            try
            {
                RaiseInteract(_input);

                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void OpenSettings_WhenMenuOpen_ClosesFirstMenu()
        {
            var router = CreateRouter();
            try
            {
                _menu.Open();

                RaiseOpenSettings(_input);

                Assert.That(_menu.CloseCount, Is.EqualTo(1));
                Assert.That(_settings.IsOpen.Value, Is.False);
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void OpenSettings_WhenNoMenuOpen_TogglesSettingsMenu()
        {
            var router = CreateRouter();
            try
            {
                RaiseOpenSettings(_input);

                Assert.That(_settings.IsOpen.Value, Is.True);
                Assert.That(Time.timeScale, Is.EqualTo(0f));
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void OpenMushroomBook_TogglesBookMenu()
        {
            var router = CreateRouter();
            try
            {
                RaiseOpenBook(_input);

                Assert.That(_book.IsOpen.Value, Is.True);
            }
            finally
            {
                router.Dispose();
            }
        }

        [Test]
        public void Dispose_UnsubscribesFromAdventureInput()
        {
            var router = CreateRouter();
            try
            {
                RaiseMove(_input, new Vector2(1f, 1f));
                _movementController.ResetExternalInput();

                router.Dispose();
                router = null;

                RaiseMove(_input, new Vector2(0.25f, 0.75f));

                Assert.That(GetMovementField<Vector2>("_moveInput"), Is.EqualTo(Vector2.zero));
            }
            finally
            {
                router?.Dispose();
            }
        }

        private AdventureInputRouter CreateRouter()
        {
            var router = new AdventureInputRouter();
            SetField(router, "adventureCharacterInput", _input);
            SetField(router, "movementController", _movementController);
            SetField(router, "interactionController", _interactionController);
            SetField(router, "inputModeVM", _inputMode);
            SetField(router, "menusVM", _menus);
            SetField(router, "settingsVM", _settings);
            SetField(router, "bookVM", _book);
            InvokeMethod(router, "Construct");
            return router;
        }

        private PlayerMovementController CreateMovementController(out GameObject go)
        {
            go = new GameObject("MovementController");
            go.AddComponent<CharacterController>();
            _movementSettings = ScriptableObject.CreateInstance<MovementSettingsSO>();
            LogAssert.Expect(LogType.Error, "MovementSettingsSO not assigned");
            var controller = go.AddComponent<PlayerMovementController>();
            controller.enabled = true;
            SetField(controller, "settings", _movementSettings);
            InvokeMethod(controller, "Awake");
            return controller;
        }

        private static PlayerInteractionController CreateInteractionController(out GameObject go, AdventureInput input)
        {
            go = new GameObject("InteractionController");
            var controller = go.AddComponent<PlayerInteractionController>();
            controller.enabled = false;
            SetField(controller, "playerInput", input);
            return controller;
        }

        private T GetMovementField<T>(string name)
        {
            return (T)_movementController.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_movementController);
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);
        }

        private static void InvokeMethod(object target, string name)
        {
            target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
                .Invoke(target, null);
        }

        private static void RaiseMove(AdventureInput input, Vector2 value) => Raise(input, "MoveChanged", value);

        private static void RaiseLook(AdventureInput input, Vector2 value) => Raise(input, "LookChanged", value);

        private static void RaiseSprint(AdventureInput input, bool value) => Raise(input, "SprintChanged", value);

        private static void RaiseInteract(AdventureInput input) => Raise(input, "InteractPerformed");

        private static void RaiseOpenSettings(AdventureInput input) => Raise(input, "OpenSettingsPerformed");

        private static void RaiseOpenBook(AdventureInput input) => Raise(input, "OpenMushroomBookPerformed");

        private static void Raise<T>(AdventureInput input, string fieldName, T arg)
        {
            var field = typeof(AdventureInput).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var action = field?.GetValue(input) as Action<T>;
            action?.Invoke(arg);
        }

        private static void Raise(AdventureInput input, string fieldName)
        {
            var field = typeof(AdventureInput).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var action = field?.GetValue(input) as Action;
            action?.Invoke();
        }

        private sealed class InputModeStub : IInputModeVM
        {
            public InputMode Current { get; private set; } = InputMode.Enabled;
            public bool CanMove { get; set; } = true;
            public bool CanLook { get; set; } = true;
            public bool IsCursorVisible => false;
            public event Action<InputMode> OnModeChanged;

            public void PushMode(InputMode mode)
            {
                Current = mode;
                OnModeChanged?.Invoke(Current);
            }

            public void PopMode(InputMode mode)
            {
                if (Current == mode)
                {
                    Current = InputMode.Enabled;
                    OnModeChanged?.Invoke(Current);
                }
            }
        }

        private sealed class TestActiveMenu : IActiveMenu
        {
            private readonly ReactiveProperty<bool> _isOpen = new(false);

            public TestActiveMenu(InputMode inputMode)
            {
                InputMode = inputMode;
            }

            public int CloseCount { get; private set; }
            public InputMode InputMode { get; }
            public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;

            public void Open()
            {
                _isOpen.SetValueAndForceNotify(true);
            }

            public void Close()
            {
                CloseCount++;
                _isOpen.SetValueAndForceNotify(false);
            }
        }

        private sealed class StubAchievementEventBus : IGameplayEventBus
        {
            public List<MushroomCollectedEvent> Collected { get; } = new();
            public List<BattleCompletedEvent> Battles { get; } = new();

            public IObservable<MushroomCollectedEvent> MushroomCollectedStream => Observable.Empty<MushroomCollectedEvent>();
            public IObservable<BattleCompletedEvent> BattleCompletedStream => Observable.Empty<BattleCompletedEvent>();

            public void PublishMushroomCollected(UnitType type, IReadOnlyDictionary<UnitType, int> totals)
            {
                Collected.Add(new MushroomCollectedEvent(type, totals));
            }

            public void PublishBattleCompleted(bool playerWon)
            {
                Battles.Add(new BattleCompletedEvent(playerWon));
            }
        }
    }
}







