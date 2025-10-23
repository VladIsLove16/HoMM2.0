using System;
using UniRx;
using Adventure.Settings.Model;
using UnityEngine;

namespace Adventure.Settings.ViewModel
{
    public class GameSettingsViewModel : IActiveMenu
    {
        private readonly GameSettingsModel _model;
        private readonly PauseController _pauseController;

        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        private readonly ReactiveProperty<bool> _isOpen = new(false);

        public AudioSettingsModel Audio => _model.Audio;
        public GraphicsSettingsModel Graphics => _model.Graphics;
        public ControlSettingsModel Controls => _model.Controls;

        public InputMode InputMode => InputMode.Blocked;

        public GameSettingsViewModel(GameSettingsModel model, PauseController pauseController)
        {
            _model = model;
            _pauseController = pauseController;
        }

        public void Open()
        {
            if (_isOpen.Value) return;
            _isOpen.Value = true;
            _pauseController.SetPaused(true);
        }

        public void Close()
        {
            if (!_isOpen.Value) return;
            _isOpen.Value = false;
            _pauseController.SetPaused(false);
        }

        public void Toggle()
        {
            if (_isOpen.Value)
                Close();
            else
                Open();
        }

        public void ApplyGraphics()
        {
            UnityEngine.QualitySettings.SetQualityLevel(_model.Graphics.QualityLevel);
            Screen.SetResolution(_model.Graphics.ResolutionWidth, _model.Graphics.ResolutionHeight, _model.Graphics.Fullscreen);
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        internal void SetQualityLevel(int i)
        {
            Debug.Log("new quality level " + i);
        }

        internal void SetMusicVolume(float v)
        {
            Audio.MusicVolume = v;
        }

        internal void SetEffectsVolume(float v)
        {
            Audio.EffectsVolume = v;
        }
    }
}
