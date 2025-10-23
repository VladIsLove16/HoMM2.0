using System;
using System.Diagnostics;
using UniRx;
using Zenject;

namespace Adventure.Infrastructure.Cursor
{
    public class CursorViewModel : ICursorViewModel
    {
        public IReadOnlyReactiveProperty<CursorVisualState> CursorState => _cursorState;
        private readonly ReactiveProperty<CursorVisualState> _cursorState = new(CursorVisualState.Hidden);
        private InputModeViewModel _inputModeViewModel;
        public IReadOnlyReactiveProperty<bool> IsLocked => _isLocked;

        private readonly ReactiveProperty<bool> _isLocked = new(true);
        [Inject]
        private void Construct(InputModeViewModel inputModeViewModel)
        {
            _inputModeViewModel = inputModeViewModel;
            inputModeViewModel.OnModeChanged += OnInputModeChanged;
            OnInputModeChanged(inputModeViewModel.Current);
        }
        private void OnInputModeChanged(InputMode mode)
        {
            UnityLogger.Log("CursorViewModel - Input mode changed to " + mode);
            if (_inputModeViewModel.CanLook)
                LockToCenter();
            else
                Unlock();
        }

        public void LockToCenter()
        {
            _isLocked.SetValueAndForceNotify(true);
            _cursorState.SetValueAndForceNotify(CursorVisualState.Hidden);
        }

        public void Unlock(CursorVisualState visual = CursorVisualState.Default)
        {
            _isLocked.SetValueAndForceNotify(false);
            _cursorState.SetValueAndForceNotify(visual);
        }
    }
}
