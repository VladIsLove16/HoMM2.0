using NUnit.Framework;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode.Views
{
    [TestFixture]
    public class CursorView_EditModeTests
    {
        private CursorView _view;
        private TestCursorViewModel _viewModel;
        private List<Texture2D> _allocatedTextures;

        [SetUp]
        public void SetUp()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _allocatedTextures = new List<Texture2D>();
            _viewModel = new TestCursorViewModel();
            _view = new CursorView();

            var textures = new List<CursorStateTexture>
            {
                CreateMapping(CursorVisualState.Default, CreateTexture()),
                CreateMapping(CursorVisualState.Hidden, CreateTexture()),
                new CursorStateTexture { state = CursorVisualState.Attack, texture = null }
            };

            _view.Construct(textures, _viewModel);
        }

        [TearDown]
        public void TearDown()
        {
            if (_allocatedTextures != null)
            {
                foreach (var texture in _allocatedTextures)
                {
                    if (texture != null)
                        Object.DestroyImmediate(texture);
                }
            }

            _viewModel?.Dispose();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        [Test]
        public void HiddenState_HidesCursor()
        {
            _viewModel.SetState(CursorVisualState.Hidden);

            Assert.That(Cursor.visible, Is.False);
        }

        [Test]
        public void NonHiddenState_ShowsCursor()
        {
            _viewModel.SetState(CursorVisualState.Hidden);

            _viewModel.SetState(CursorVisualState.Default);

            Assert.That(Cursor.visible, Is.True);
        }

        [Test]
        public void LockState_LocksCursor()
        {
            _viewModel.SetLocked(true);

            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.Locked));
        }

        [Test]
        public void UnlockState_ReleasesLock()
        {
            _viewModel.SetLocked(true);

            _viewModel.SetLocked(false);

            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
        }

        [Test]
        public void NullTexture_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, "Attempting to set null cursor texture");

            _viewModel.SetState(CursorVisualState.Attack);
        }

        private CursorStateTexture CreateMapping(CursorVisualState state, Texture2D texture)
        {
            return new CursorStateTexture { state = state, texture = texture };
        }

        private Texture2D CreateTexture()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            _allocatedTextures.Add(texture);
            return texture;
        }

        private sealed class TestCursorViewModel : ICursorViewModel
        {
            private readonly ReactiveProperty<CursorVisualState> _cursorState;
            private readonly ReactiveProperty<bool> _isLocked;

            public TestCursorViewModel()
            {
                _cursorState = new ReactiveProperty<CursorVisualState>(CursorVisualState.Default);
                _isLocked = new ReactiveProperty<bool>(false);
            }

            public void SetState(CursorVisualState state) => _cursorState.SetValueAndForceNotify(state);

            public void SetLocked(bool locked) => _isLocked.SetValueAndForceNotify(locked);

            public void Dispose()
            {
                _cursorState.Dispose();
                _isLocked.Dispose();
            }

            IReadOnlyReactiveProperty<CursorVisualState> ICursorViewModel.CursorState => _cursorState;

            IReadOnlyReactiveProperty<bool> ICursorViewModel.IsLocked => _isLocked;
        }
    }
}
