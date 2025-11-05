using Adventure.Infrastructure.Persistence;
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
        private const string MasterVolumeParam = "Master";
        private const string MusicVolumeParam = "Music";
        private const string EffectsVolumeParam = "Sounds";

        private const float MinNormalizedVolume = 0.0001f;
        private const float MutedDecibels = -80f;

        private readonly GameSettingsModel _model;
        private readonly PauseController _pauseController;
        private readonly IAnimationSpeedSettings _animationSpeedSettings;
        private readonly AudioMixer _audioMixer;
        private readonly IDataRepository<GameSettingsSaveData> _settingsRepository;
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;

        public AudioSettingsModel Audio => _model.Audio;
        public GraphicsSettingsModel Graphics => _model.Graphics;
        public ControlSettingsModel Controls => _model.Controls;
        public IReadOnlyReactiveProperty<AnimationSpeedMode> AnimationSpeed => _animationSpeedSettings.Mode;

        public InputMode InputMode => InputMode.Blocked;

        [Inject]
        public GameSettingsViewModel(
            GameSettingsModel model,
            PauseController pauseController,
            IAnimationSpeedSettings animationSpeedSettings,
            AudioMixer audioMixer,
            IDataRepository<GameSettingsSaveData> settingsRepository)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _pauseController = pauseController ?? throw new ArgumentNullException(nameof(pauseController));
            _animationSpeedSettings = animationSpeedSettings ?? throw new ArgumentNullException(nameof(animationSpeedSettings));
            _audioMixer = audioMixer ?? throw new ArgumentNullException(nameof(audioMixer));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));

            LoadSettings();
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
            PersistSettings();
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
            int maxIndex = (QualitySettings.names != null && QualitySettings.names.Length > 0)
                ? QualitySettings.names.Length - 1
                : i;
            Graphics.QualityLevel = Mathf.Clamp(i, 0, Mathf.Max(0, maxIndex));
            PersistSettings();
            Debug.Log("new quality level " + Graphics.QualityLevel);
        }

        internal void SetMusicVolume(float v)
        {
            ApplyVolume(MusicVolumeParam, v, value => Audio.MusicVolume = value);
        }

        internal void SetSoundsVolume(float v)
        {
            ApplyVolume(EffectsVolumeParam, v, value => Audio.EffectsVolume = value);
        }

        internal void SetAnimationSpeed(AnimationSpeedMode mode)
        {
            _animationSpeedSettings.SetMode(mode);
            PersistSettings();
        }

        internal void SetMasterVolume(float v)
        {
            ApplyVolume(MasterVolumeParam, v, value => Audio.MasterVolume = value);
        }

        internal void SetMixerGroupVolume(string exposedParameter, float value)
        {
            if (string.IsNullOrWhiteSpace(exposedParameter))
                throw new ArgumentException("Exposed parameter name must be provided.", nameof(exposedParameter));

            ApplyVolumeInternal(exposedParameter, value);
        }

        internal void SetMixerGroupVolume(AudioMixerGroup group, string exposedParameter, float value)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            if (string.IsNullOrWhiteSpace(exposedParameter))
                throw new ArgumentException("Exposed parameter name must be provided.", nameof(exposedParameter));

            var targetMixer = group.audioMixer != null ? group.audioMixer : _audioMixer;
            ApplyVolumeInternal(exposedParameter, value, targetMixer);
        }

        private void LoadSettings()
        {
            var data = _settingsRepository.Load() ?? new GameSettingsSaveData();

            ApplyVolume(MasterVolumeParam, data.MasterVolume, v => Audio.MasterVolume = v, persist: false);
            ApplyVolume(MusicVolumeParam, data.MusicVolume, v => Audio.MusicVolume = v, persist: false);
            ApplyVolume(EffectsVolumeParam, data.EffectsVolume, v => Audio.EffectsVolume = v, persist: false);

            Graphics.QualityLevel = data.QualityLevel;
            Graphics.ResolutionWidth = data.ResolutionWidth;
            Graphics.ResolutionHeight = data.ResolutionHeight;
            Graphics.Fullscreen = data.Fullscreen;

            _animationSpeedSettings.SetMode(data.AnimationSpeed);
        }

        private void ApplyVolume(string exposedParam, float value, Action<float> assign, bool persist = true)
        {
            var normalized = Mathf.Clamp01(value);
            assign(normalized);
            ApplyVolumeInternal(exposedParam, normalized);

            if (persist)
            {
                PersistSettings();
            }
        }

        private void ApplyVolumeInternal(string exposedParam, float normalized, AudioMixer targetMixer = null)
        {
            var clamped = Mathf.Clamp(normalized, MinNormalizedVolume, 1f);
            var decibels = Mathf.Approximately(normalized, 0f) ? MutedDecibels : Mathf.Log10(clamped) * 20f;
            (targetMixer ?? _audioMixer).SetFloat(exposedParam, decibels);
        }

        private void PersistSettings()
        {
            if (_settingsRepository == null)
                return;

            var data = new GameSettingsSaveData
            {
                MasterVolume = Audio.MasterVolume,
                MusicVolume = Audio.MusicVolume,
                EffectsVolume = Audio.EffectsVolume,
                QualityLevel = Graphics.QualityLevel,
                ResolutionWidth = Graphics.ResolutionWidth,
                ResolutionHeight = Graphics.ResolutionHeight,
                Fullscreen = Graphics.Fullscreen,
                AnimationSpeed = _animationSpeedSettings.Mode.Value
            };

            _settingsRepository.Save(data);
        }
    }
}
