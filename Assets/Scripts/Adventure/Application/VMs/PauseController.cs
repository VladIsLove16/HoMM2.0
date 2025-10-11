using UniRx;
using UnityEngine;

namespace Adventure.Settings.ViewModel
{
    public sealed class PauseController
    {
        private readonly ReactiveProperty<bool> _isPaused = new(false);
        public IReadOnlyReactiveProperty<bool> IsPaused => _isPaused;

        public void SetPaused(bool paused)
        {
            if (_isPaused.Value == paused)
                return;

            _isPaused.Value = paused;
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
