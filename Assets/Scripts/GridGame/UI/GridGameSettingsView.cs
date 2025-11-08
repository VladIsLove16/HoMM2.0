using Adventure.Settings.ViewModel;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Settings.View
{
    public sealed class GridGameSettingsView : GameSettingsViewBase<GridGameSettingsViewModel>
    {
        private static readonly string[] DefaultAnimationLabels = { "1x", "2x", "Instant" };

        [SerializeField] private TMP_Dropdown animationSpeedDropdown;
        [SerializeField] private List<string> animationModeStringOptions;
        private Action<AnimationSpeedMode> _animationSetter;

        public void ConfigureAnimationControls()
        {
            if (animationModeStringOptions == null || animationModeStringOptions.Count == 0)
            {
                animationModeStringOptions = new List<string>(DefaultAnimationLabels);
            }
            if (!ViewModel.SupportsAnimationSpeed)
            {
                return;
            }

            SetupAnimationDropdown(
                ViewModel.AnimationSpeed,
                ViewModel.SetAnimationSpeed,
                animationModeStringOptions);
        }
        public override void TryInitialize()
        {
            base.TryInitialize();
            ConfigureAnimationControls();
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
                optionData.Add(new TMP_Dropdown.OptionData(GetAnimationLabel(options, index)));
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
        }

        private static string GetAnimationLabel(IList<string> options, int index)
        {
            if (options != null && index < options.Count)
            {
                var candidate = options[index];
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate;
                }
            }

            if (index < DefaultAnimationLabels.Length)
            {
                return DefaultAnimationLabels[index];
            }

            return $"Option {index + 1}";
        }
        
    }
}
