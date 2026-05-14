using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Settings.View
{
    public sealed class CloseSettingsButtonSection : GameSettingsSectionBase
    {
        [SerializeField] private Button closeButton;

        public override GameSettingsSectionFeature Feature => GameSettingsSectionFeature.CloseMenu;

        protected override void OnBind()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected override void OnUnbind()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        private void OnCloseClicked()
        {
            ViewModel.Close();
        }
    }
}
