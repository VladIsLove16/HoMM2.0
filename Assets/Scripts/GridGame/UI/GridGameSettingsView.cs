using Adventure.Settings.ViewModel;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Adventure.Settings.View
{
    public sealed class GridGameSettingsView : GameSettingsViewBase<GridGameSettingsViewModel>
    {
        [SerializeField] private TMP_Dropdown animationSpeedDropdown;
        private Action<AnimationSpeedMode> _animationSetter;
        private bool _animationConfigured;
        [SerializeField] LocalizedString animationSpeedOption1;
        [SerializeField] LocalizedString animationSpeedOption2;
        [SerializeField] LocalizedString animationSpeedOption3;

        public void ConfigureAnimationControls()
        {
            if (_animationConfigured)
                return;

            if (ViewModel == null || animationSpeedDropdown == null)
                return;

            if (!ViewModel.SupportsAnimationSpeed)
            {
                animationSpeedDropdown.gameObject.SetActive(false);
                return;
            }

            var labels = BuildAnimationLabels();
            SetupAnimationDropdown(
                ViewModel.AnimationSpeed,
                ViewModel.SetAnimationSpeed,
                labels);
            _animationConfigured = true;
        }
        public override void TryInitialize()
        {
            base.TryInitialize();
            if (ViewModel != null)
            {
                ConfigureAnimationControls();
            }
        }

        public void SetupAnimationDropdown(IReadOnlyReactiveProperty<AnimationSpeedMode> source, Action<AnimationSpeedMode> setter, IList<string> options)
        {
            if (animationSpeedDropdown == null)
                return;

            animationSpeedDropdown.gameObject.SetActive(true);
            animationSpeedDropdown.ClearOptions();
            var enumLength = Enum.GetValues(typeof(AnimationSpeedMode)).Length;
            var optionData = new List<TMP_Dropdown.OptionData>(enumLength);
            for (var index = 0; index < enumLength; index++)
            {
                optionData.Add(new TMP_Dropdown.OptionData(options[index]));
            }
            animationSpeedDropdown.AddOptions(optionData);
            animationSpeedDropdown.SetValueWithoutNotify((int)source.Value);
            animationSpeedDropdown.onValueChanged.AddListener(OnAnimationDropdownChanged);
            source.Subscribe(mode => animationSpeedDropdown.SetValueWithoutNotify((int)mode)).AddTo(Bindings);

            void SetterWrapper(AnimationSpeedMode mode) => setter(mode);
            _animationSetter = SetterWrapper;
        }
        private void OnAnimationDropdownChanged(int index)
        {
            _animationSetter?.Invoke((AnimationSpeedMode)index);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (animationSpeedDropdown != null)
                animationSpeedDropdown.onValueChanged.RemoveListener(OnAnimationDropdownChanged);
            _animationConfigured = false;
        }


        private IList<string> BuildAnimationLabels()
        {
            var labels = new List<string>();
            labels.Add(animationSpeedOption1.GetLocalizedString());
            labels.Add(animationSpeedOption2.GetLocalizedString());
            labels.Add(animationSpeedOption3.GetLocalizedString());
            return labels;
        }
        
    }
}
