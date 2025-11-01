using Adventure.Settings.Model;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Adventure.Settings.ViewModel
{
    public class GameSettingsViewModel : IActiveMenu
    {
        private readonly GameSettingsModel _model;
        private readonly PauseController _pauseController;
        private readonly IAnimationSpeedSettings _animationSpeedSettings;
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        [Inject] private AudioMixer audioMixer;
        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;

        public AudioSettingsModel Audio => _model.Audio;
        public GraphicsSettingsModel Graphics => _model.Graphics;
        public ControlSettingsModel Controls => _model.Controls;
        public IReadOnlyReactiveProperty<AnimationSpeedMode> AnimationSpeed => _animationSpeedSettings.Mode;

        public InputMode InputMode => InputMode.Blocked;

        [Inject]
        public GameSettingsViewModel(GameSettingsModel model, PauseController pauseController, IAnimationSpeedSettings animationSpeedSettings)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _pauseController = pauseController ?? throw new ArgumentNullException(nameof(pauseController));
            _animationSpeedSettings = animationSpeedSettings ?? throw new ArgumentNullException(nameof(animationSpeedSettings));
        }

        public void Open()
        {
            if (_isOpen.Value) return;
            _isOpen.SetValueAndForceNotify(true);
            _pauseController.SetPaused(true);
        }

        public void Close()
        {
            if (!_isOpen.Value) return;
            _isOpen.SetValueAndForceNotify(false);
            _pauseController.SetPaused(false);
        }

        public void Toggle()
        {
            UnityLogger.Log("GameSettingsViewModel Toggle");
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
            audioMixer.SetFloat("Volume (of Music)", Mathf.Log10(v) * 20);
            Audio.MusicVolume = v;
        }

        internal void SetSoundsVolume(float v)
        {
            audioMixer.SetFloat("Volume (of Sounds)", Mathf.Log10(v) * 20);
            Audio.EffectsVolume = v;
        }

        internal void SetAnimationSpeed(AnimationSpeedMode mode)
        {
            _animationSpeedSettings.SetMode(mode);
        }
    }
}
