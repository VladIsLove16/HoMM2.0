using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Settings.View
{
    public sealed class ExitGameButtonSection : GameSettingsSectionBase
    {
        [SerializeField] private Button exitButton;

        public override GameSettingsSectionFeature Feature => GameSettingsSectionFeature.ExitGame;

        protected override void OnBind()
        {
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        protected override void OnUnbind()
        {
            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);
        }

        private void OnExitClicked()
        {
            ViewModel.ExitGame();
        }
    }
}
