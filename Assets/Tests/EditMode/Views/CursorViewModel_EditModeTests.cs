using System.Reflection;
using Adventure.Infrastructure.Cursor;
using NUnit.Framework;

namespace Tests.EditMode.Views
{
    [TestFixture]
    public class CursorViewModel_EditModeTests
    {
        private InputModeViewModel _inputMode;
        private CursorViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _inputMode = new InputModeViewModel();
            _viewModel = CreateViewModel(_inputMode);
        }

        [Test]
        public void Construct_WhenLookAllowed_StartsLockedAndHidden()
        {
            Assert.That(_viewModel.IsLocked.Value, Is.True);
            Assert.That(_viewModel.CursorState.Value, Is.EqualTo(CursorVisualState.Hidden));
        }

        [Test]
        public void InputModeWithoutLook_UnlocksCursor()
        {
            _inputMode.PushMode(InputMode.Blocked);

            Assert.That(_viewModel.IsLocked.Value, Is.False);
            Assert.That(_viewModel.CursorState.Value, Is.EqualTo(CursorVisualState.Default));
        }

        [Test]
        public void InputModeRestoringLook_ReLocksCursor()
        {
            _inputMode.PushMode(InputMode.OnlyMove);
            _inputMode.PopMode(InputMode.OnlyMove);

            Assert.That(_viewModel.IsLocked.Value, Is.True);
            Assert.That(_viewModel.CursorState.Value, Is.EqualTo(CursorVisualState.Hidden));
        }

        [Test]
        public void Unlock_WithCustomVisual_UpdatesState()
        {
            _viewModel.Unlock(CursorVisualState.Attack);

            Assert.That(_viewModel.IsLocked.Value, Is.False);
            Assert.That(_viewModel.CursorState.Value, Is.EqualTo(CursorVisualState.Attack));
        }

        [Test]
        public void LockToCenter_AlwaysHidesCursor()
        {
            _viewModel.Unlock(CursorVisualState.Move);

            _viewModel.LockToCenter();

            Assert.That(_viewModel.IsLocked.Value, Is.True);
            Assert.That(_viewModel.CursorState.Value, Is.EqualTo(CursorVisualState.Hidden));
        }

        private static CursorViewModel CreateViewModel(InputModeViewModel inputMode)
        {
            var viewModel = new CursorViewModel();
            var construct = typeof(CursorViewModel).GetMethod("Construct", BindingFlags.Instance | BindingFlags.NonPublic);
            construct.Invoke(viewModel, new object[] { inputMode });
            return viewModel;
        }
    }
}
