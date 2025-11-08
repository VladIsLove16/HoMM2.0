using Adventure.Infrastructure.Persistence;
using Adventure.Settings.Model;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

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
        private readonly AudioMixer _audioMixer;
        private readonly IDataRepository<GameSettingsSaveData> _settingsRepository;
        private readonly ReactiveProperty<bool> _isOpen = new(false);
        private bool loaded = false;
        public GameSettingsViewModel(
            GameSettingsModel model,
            PauseController pauseController,
            IDataRepository<GameSettingsSaveData> settingsRepository,
            AudioMixer audioMixer)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _pauseController = pauseController ?? throw new ArgumentNullException(nameof(pauseController));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _audioMixer = audioMixer;

        }

        public IReadOnlyReactiveProperty<bool> IsOpen => _isOpen;
        public AudioSettingsModel Audio => _model.Audio;
        public GraphicsSettingsModel Graphics => _model.Graphics;
        public ControlSettingsModel Controls => _model.Controls;

        public virtual void Open()
        {
            LoadSettings();

            if (_isOpen.Value)
                return;

            _isOpen.SetValueAndForceNotify(true);
            _pauseController.SetPaused(true);
        }

        public virtual void Close()
        {
            if (!_isOpen.Value)
                return;

            _isOpen.SetValueAndForceNotify(false);
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
            QualitySettings.SetQualityLevel(_model.Graphics.QualityLevel);
            Screen.SetResolution(_model.Graphics.ResolutionWidth, _model.Graphics.ResolutionHeight, _model.Graphics.Fullscreen);
            PersistSettings();
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetQualityLevel(int level)
        {
            var maxIndex = QualitySettings.names != null && QualitySettings.names.Length > 0
                ? QualitySettings.names.Length - 1
                : level;
            Graphics.QualityLevel = Mathf.Clamp(level, 0, Mathf.Max(0, maxIndex));
            PersistSettings();
        }

        public void SetMusicVolume(float value)
        {
            ApplyVolume(MusicVolumeParam, value, v => Audio.MusicVolume = v);
        }

        public void SetSoundsVolume(float value)
        {
            ApplyVolume(EffectsVolumeParam, value, v => Audio.EffectsVolume = v);
        }

        public void SetMasterVolume(float value)
        {
            ApplyVolume(MasterVolumeParam, value, v => Audio.MasterVolume = v);
        }

        public void SetMixerGroupVolume(string exposedParameter, float value)
        {
            if (string.IsNullOrWhiteSpace(exposedParameter))
                throw new ArgumentException("Exposed parameter name must be provided.", nameof(exposedParameter));

            ApplyVolumeInternal(exposedParameter, value);
        }

        public void SetMixerGroupVolume(AudioMixerGroup group, string exposedParameter, float value)
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
            if (loaded)
                return;
            var data = _settingsRepository.Load() ?? new GameSettingsSaveData();

            ApplyVolume(MasterVolumeParam, data.MasterVolume, v => Audio.MasterVolume = v, persist: false);
            ApplyVolume(MusicVolumeParam, data.MusicVolume, v => Audio.MusicVolume = v, persist: false);
            ApplyVolume(EffectsVolumeParam, data.EffectsVolume, v => Audio.EffectsVolume = v, persist: false);

            Graphics.QualityLevel = data.QualityLevel;
            Graphics.ResolutionWidth = data.ResolutionWidth;
            Graphics.ResolutionHeight = data.ResolutionHeight;
            Graphics.Fullscreen = data.Fullscreen;
            OnLoadAdditionalSettings(data);
            loaded = true;
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
            var mixer = targetMixer ?? _audioMixer;
            if (mixer == null)
                return;

            var clamped = Mathf.Clamp(normalized, MinNormalizedVolume, 1f);
            var decibels = Mathf.Approximately(normalized, 0f) ? MutedDecibels : Mathf.Log10(clamped) * 20f;
            mixer.SetFloat(exposedParam, decibels);
        }

        protected void PersistSettings()
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
            };

            PersistAdditionalSettings(data);
            _settingsRepository.Save(data);
        }

        protected virtual void OnLoadAdditionalSettings(GameSettingsSaveData data)
        {
        }

        protected virtual void PersistAdditionalSettings(GameSettingsSaveData data)
        {
        }
    }
}
