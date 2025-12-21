using Adventure.Settings.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Adventure.Settings.View
{
    public sealed class AdventureGameSettingsView : GameSettingsViewBase<AdventureGameSettingsViewModel>
    {
        [Header("Controls")]
        [SerializeField] private Slider mouseSensitivitySlider;

        private bool _mouseSliderBound;

        [Inject]
        public override void Construct(AdventureGameSettingsViewModel vm)
        {
            base.Construct(vm);
            BindMouseSensitivitySlider();
        }

        protected override void Start()
        {
            base.Start();
            BindMouseSensitivitySlider();
        }

        public override void TryInitialize()
        {
            base.TryInitialize();
            BindMouseSensitivitySlider();
        }

        protected override void OnDestroy()
        {
            if (_mouseSliderBound && mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
                _mouseSliderBound = false;
            }
            base.OnDestroy();
        }

        private void BindMouseSensitivitySlider()
        {
            if (_mouseSliderBound || mouseSensitivitySlider == null || ViewModel == null)
                return;

            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            mouseSensitivitySlider.SetValueWithoutNotify(ViewModel.Controls.MouseSensitivity);
            _mouseSliderBound = true;
        }

        private void OnMouseSensitivityChanged(float value)
        {
            ViewModel.SetMouseSensitivity(value);
        }
    }
}
