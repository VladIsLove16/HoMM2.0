using TMPro;
using UnityEngine;

namespace Adventure.Settings.View
{
    public sealed class QualitySettingsSection : GameSettingsSectionBase
    {
        [SerializeField] private TMP_Dropdown qualityDropdown;

        public override GameSettingsSectionFeature Feature => GameSettingsSectionFeature.Quality;

        protected override void OnBind()
        {
            if (qualityDropdown == null)
                return;

            qualityDropdown.SetValueWithoutNotify(ViewModel.Graphics.QualityLevel);
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        protected override void OnUnbind()
        {
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
        }

        private void OnQualityChanged(int value)
        {
            ViewModel.SetQualityLevel(value);
            ViewModel.ApplyGraphics();
        }
    }
}
