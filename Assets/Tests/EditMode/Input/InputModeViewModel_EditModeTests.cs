using NUnit.Framework;

namespace Tests.EditMode.Input
{
    [TestFixture]
    public class InputModeViewModel_EditModeTests
    {
        private InputModeViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _viewModel = new InputModeViewModel();
        }

        [Test]
        public void DefaultState_AllowsMoveAndLook()
        {
            Assert.That(_viewModel.Current, Is.EqualTo(InputMode.Enabled));
            Assert.That(_viewModel.CanMove, Is.True);
            Assert.That(_viewModel.CanLook, Is.True);
            Assert.That(_viewModel.IsCursorVisible, Is.False);
        }

        [Test]
        public void PushMode_UpdatesStateAndNotifies()
        {
            InputMode? observed = null;
            _viewModel.OnModeChanged += mode => observed = mode;

            _viewModel.PushMode(InputMode.OnlyMove);

            Assert.That(_viewModel.Current, Is.EqualTo(InputMode.OnlyMove));
            Assert.That(observed, Is.EqualTo(InputMode.OnlyMove));
            Assert.That(_viewModel.CanMove, Is.True);
            Assert.That(_viewModel.CanLook, Is.False);
            Assert.That(_viewModel.IsCursorVisible, Is.True);
        }

        [Test]
        public void PopMode_RestoresPreviousMode()
        {
            _viewModel.PushMode(InputMode.Blocked);
            InputMode? observed = null;
            _viewModel.OnModeChanged += mode => observed = mode;

            _viewModel.PopMode(InputMode.Blocked);

            Assert.That(_viewModel.Current, Is.EqualTo(InputMode.Enabled));
            Assert.That(observed, Is.EqualTo(InputMode.Enabled));
        }

        [Test]
        public void PopMode_RemovesAllOccurrences()
        {
            _viewModel.PushMode(InputMode.OnlyMove);
            _viewModel.PushMode(InputMode.Blocked);
            _viewModel.PushMode(InputMode.OnlyMove);

            _viewModel.PopMode(InputMode.OnlyMove);

            Assert.That(_viewModel.Current, Is.EqualTo(InputMode.Blocked));
        }

        [Test]
        public void PopMode_WhenModeMissing_DoesNothing()
        {
            InputMode? observed = null;
            _viewModel.OnModeChanged += mode => observed = mode;

            _viewModel.PopMode(InputMode.OnlyLook);

            Assert.That(_viewModel.Current, Is.EqualTo(InputMode.Enabled));
            Assert.That(observed, Is.Null);
        }
    }
}
