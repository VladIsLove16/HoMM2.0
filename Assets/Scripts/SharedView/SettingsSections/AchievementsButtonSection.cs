using Game.Achievements;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Adventure.Settings.ViewModel;

namespace Adventure.Settings.View
{
    public sealed class AchievementsButtonSection : GameSettingsSectionBase
    {
        [SerializeField] private Button achievementsButton;

        private IAchievementsNavigation _achievementsNavigation;
        private AchievementsViewModel _achievementsViewModel;

        public override GameSettingsSectionFeature Feature => GameSettingsSectionFeature.Achievements;

        [Inject]
        private void InjectDependencies(
            [InjectOptional] IAchievementsNavigation achievementsNavigation = null,
            [InjectOptional] AchievementsViewModel achievementsViewModel = null)
        {
            _achievementsNavigation = achievementsNavigation;
            _achievementsViewModel = achievementsViewModel;
        }

        protected override void OnBind()
        {
            if (achievementsButton == null || (_achievementsNavigation == null && _achievementsViewModel == null))
                return;

            achievementsButton.onClick.AddListener(OnAchievementsClicked);
        }

        protected override void OnUnbind()
        {
            if (achievementsButton != null)
                achievementsButton.onClick.RemoveListener(OnAchievementsClicked);
        }

        private void OnAchievementsClicked()
        {
            if (_achievementsNavigation != null)
            {
                _achievementsNavigation.OpenAchievements();
                return;
            }

            _achievementsViewModel?.Toggle();
        }
    }
}
