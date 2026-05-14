using Adventure.Settings.ViewModel;
using UniRx;
using UnityEngine;

namespace Adventure.Settings.View
{
    public enum GameSettingsSectionFeature
    {
        Audio,
        Language,
        Quality,
        ExitGame,
        Achievements,
        CloseMenu
    }

    public abstract class GameSettingsSectionBase : MonoBehaviour
    {
        public abstract GameSettingsSectionFeature Feature { get; }

        protected GameSettingsViewModel ViewModel { get; private set; }
        protected CompositeDisposable Bindings { get; private set; }

        private bool _isBound;

        public void Bind(GameSettingsViewModel viewModel, CompositeDisposable parentBindings)
        {
            if (_isBound || viewModel == null)
                return;

            ViewModel = viewModel;
            Bindings = new CompositeDisposable();
            parentBindings?.Add(Bindings);
            _isBound = true;
            OnBind();
        }

        public void Unbind()
        {
            if (!_isBound)
                return;

            OnUnbind();
            Bindings?.Dispose();
            Bindings = null;
            ViewModel = null;
            _isBound = false;
        }

        protected abstract void OnBind();

        protected virtual void OnUnbind()
        {
        }
    }
}
