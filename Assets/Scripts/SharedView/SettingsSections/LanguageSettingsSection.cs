using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Adventure.Settings.View
{
    public sealed class LanguageSettingsSection : GameSettingsSectionBase
    {
        [SerializeField] private TMP_Dropdown languageDropdown;

        private bool _waitingLocalizationInit;

        public override GameSettingsSectionFeature Feature => GameSettingsSectionFeature.Language;

        protected override void OnBind()
        {
            if (languageDropdown == null)
                return;

            ConfigureLanguageDropdown();
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            ViewModel.LanguageIndex
                .Subscribe(index => languageDropdown.SetValueWithoutNotify(index))
                .AddTo(Bindings);
        }

        protected override void OnUnbind()
        {
            if (languageDropdown != null)
                languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);

            if (_waitingLocalizationInit)
            {
                LocalizationSettings.InitializationOperation.Completed -= OnLocalizationInitializationCompleted;
                _waitingLocalizationInit = false;
            }
        }

        private void OnLanguageChanged(int value)
        {
            ViewModel.LanguageIndex.SetValueAndForceNotify(value);
        }

        private void ConfigureLanguageDropdown()
        {
            if (languageDropdown == null)
                return;

            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                if (_waitingLocalizationInit)
                    return;

                _waitingLocalizationInit = true;
                LocalizationSettings.InitializationOperation.Completed += OnLocalizationInitializationCompleted;
                return;
            }

            PopulateLanguageDropdown();
        }

        private void OnLocalizationInitializationCompleted(AsyncOperationHandle<LocalizationSettings> handle)
        {
            LocalizationSettings.InitializationOperation.Completed -= OnLocalizationInitializationCompleted;
            _waitingLocalizationInit = false;
            PopulateLanguageDropdown();
        }

        private void PopulateLanguageDropdown()
        {
            if (languageDropdown == null)
                return;

            var locales = LocalizationSettings.AvailableLocales?.Locales;
            if (locales == null || locales.Count == 0)
            {
                languageDropdown.interactable = false;
                languageDropdown.ClearOptions();
                return;
            }

            var options = new List<TMP_Dropdown.OptionData>(locales.Count);
            foreach (var locale in locales)
            {
                if (locale == null)
                    continue;

                var display = !string.IsNullOrWhiteSpace(locale.LocaleName)
                    ? locale.LocaleName
                    : locale.Identifier.Code;
                options.Add(new TMP_Dropdown.OptionData(display));
            }

            languageDropdown.interactable = locales.Count > 1;
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(options);
            languageDropdown.SetValueWithoutNotify(ViewModel.LanguageIndex.Value);
        }
    }
}
