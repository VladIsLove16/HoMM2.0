using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Adventure.Infrastructure.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tests.EditMode.Input
{
    [TestFixture]
    public class AdventureInput_EditModeTests : InputTestFixture
    {
        private AdventureInput _input;

        [SetUp]
        public void SetUp()
        {
            base.Setup();
            _input = new AdventureInput();
            InvokeConstruct(_input);
        }

        [TearDown]
        public void TearDown()
        {
            _input.Dispose();
            base.TearDown();
        }

        [Test]
        public void MoveChanged_RaisedOnKeyboardComposite()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var received = new List<Vector2>();
            _input.MoveChanged += received.Add;

            Press(keyboard.wKey);
            Release(keyboard.wKey);

            Assert.That(received, Is.Not.Empty);
            Assert.That(received.First().y, Is.EqualTo(1f).Within(0.001f));
            Assert.That(received.Last(), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void LookChanged_RaisedOnMouseDelta()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var received = new List<Vector2>();
            _input.LookChanged += received.Add;

            Set(mouse.delta, new Vector2(3f, -2f));
            Set(mouse.delta, Vector2.zero);

            Assert.That(received, Is.Not.Empty);
            Assert.That(received.First(), Is.EqualTo(new Vector2(3f, -2f)));
            Assert.That(received.Last(), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void SprintChanged_RaisedOnSprintAction()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var received = new List<bool>();
            _input.SprintChanged += received.Add;

            Press(keyboard.leftShiftKey);
            Release(keyboard.leftShiftKey);

            CollectionAssert.AreEqual(new[] { true, false }, received);
        }

        [Test]
        public void InteractPerformed_RaisedOnInteractAction()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var invoked = false;
            _input.InteractPerformed += () => invoked = true;

            Press(keyboard.eKey);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void OpenSettingsPerformed_RaisedOnEscape()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var invoked = false;
            _input.OpenSettingsPerformed += () => invoked = true;

            Press(keyboard.escapeKey);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void OpenMushroomBookPerformed_RaisedOnMKey()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var invoked = false;
            _input.OpenMushroomBookPerformed += () => invoked = true;

            Press(keyboard.mKey);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void OnDisable_ResetsMoveAndSprint()
        {
            var moveValues = new List<Vector2>();
            var sprintValues = new List<bool>();
            _input.MoveChanged += moveValues.Add;
            _input.SprintChanged += sprintValues.Add;

            InvokeOnDisable(_input);

            Assert.That(moveValues, Is.EqualTo(new[] { Vector2.zero }));
            Assert.That(sprintValues, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void Dispose_UnsubscribesActions()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var received = new List<Vector2>();
            _input.MoveChanged += received.Add;

            _input.Dispose();

            Press(keyboard.wKey);

            Assert.That(received, Is.Empty);
        }

        private static void InvokeConstruct(AdventureInput input)
        {
            typeof(AdventureInput).GetMethod("Construct", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(input, null);
        }

        private static void InvokeOnDisable(AdventureInput input)
        {
            typeof(AdventureInput).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(input, null);
        }
    }
}
