using Adventure.Infrastructure.Persistence;
using Adventure.Settings.Model;
using Game.Achievements;
using UnityEngine.Audio;
using Zenject;

namespace Adventure.Settings.ViewModel
{
    public sealed class MainMenuGameSettingsViewModel : GameSettingsViewModel
    {
        private AchievementsViewModel _achievementsViewModel;

        public MainMenuGameSettingsViewModel(
            GameSettingsModel model,
            PauseController pauseController,
            IDataRepository<GameSettingsSaveData> settingsRepository,
            AudioMixer audioMixer)
            : base(model, pauseController, settingsRepository, audioMixer)
        {
            EnsureSettingsLoaded();
        }

        [Inject]
        private void InjectAchievementsViewModel([InjectOptional] AchievementsViewModel achievementsViewModel = null)
        {
            _achievementsViewModel = achievementsViewModel;
        }

        public override void Close()
        {
            _achievementsViewModel?.Close();
            base.Close();
        }
    }
}
