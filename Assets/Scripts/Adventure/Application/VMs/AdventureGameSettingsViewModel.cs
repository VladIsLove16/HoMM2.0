using Adventure.Infrastructure.Persistence;
using Adventure.Settings.Model;
using UnityEngine.Audio;

namespace Adventure.Settings.ViewModel
{
    public sealed class AdventureGameSettingsViewModel : GameSettingsViewModel, IAdventureGameActiveMenu
    {
        private readonly InputMode _inputMode = InputMode.Blocked;

        public AdventureGameSettingsViewModel(
            GameSettingsModel model,
            PauseController pauseController,
            IDataRepository<GameSettingsSaveData> settingsRepository,
            AudioMixer audioMixer)
            : base(model, pauseController, settingsRepository, audioMixer)
        {
            EnsureSettingsLoaded();
        }

        public InputMode InputMode => _inputMode;
        public override void ExitGame()
        {
            SceneLoader.Load(SceneLoader.Scene.MainMenu);
        }
    }
}
