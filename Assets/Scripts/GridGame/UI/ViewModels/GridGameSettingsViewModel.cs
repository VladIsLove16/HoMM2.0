using System;
using Adventure.Infrastructure.Persistence;
using Adventure.Settings.Model;
using UnityEngine.Audio;
using UniRx;

namespace Adventure.Settings.ViewModel
{
    public sealed class GridGameSettingsViewModel : GameSettingsViewModel, IGridGameActiveMenu
    {
        private readonly IAnimationSpeedSettings _animationSettings;

        public GridGameSettingsViewModel(
            GameSettingsModel model,
            PauseController pauseController,
            IDataRepository<GameSettingsSaveData> settingsRepository,
            AudioMixer audioMixer,
            IAnimationSpeedSettings animationSettings)
            : base(model, pauseController, settingsRepository, audioMixer)
        {
            _animationSettings = animationSettings ?? throw new ArgumentNullException(nameof(animationSettings));
        }

        public bool SupportsAnimationSpeed => true;
        public IReadOnlyReactiveProperty<AnimationSpeedMode> AnimationSpeed => _animationSettings.Mode;

        public void SetAnimationSpeed(AnimationSpeedMode mode)
        {
            _animationSettings.SetMode(mode);
            PersistSettings();
        }

        protected override void OnLoadAdditionalSettings(GameSettingsSaveData data)
        {
            _animationSettings.SetMode(data.AnimationSpeed);
        }

        protected override void PersistAdditionalSettings(GameSettingsSaveData data)
        {
            data.AnimationSpeed = _animationSettings.Mode.Value;
        }
    }
}
