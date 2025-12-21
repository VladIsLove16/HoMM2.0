using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Shared.Localization
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class LanguageDropdownBinder : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        private readonly List<Locale> _locales = new List<Locale>();
        private bool _suppressCallback;

        private void Awake()
        {
            if (dropdown == null)
                dropdown = GetComponent<TMP_Dropdown>();

            LocalizationSettings.InitializationOperation.WaitForCompletion();
        }

        private void OnEnable()
        {
            ConfigureOptions();
            ApplySelection(LocalizationSettings.SelectedLocale);

            dropdown.onValueChanged.AddListener(OnDropdownChanged);
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private void OnDisable()
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        private void ConfigureOptions()
        {
            _locales.Clear();
            dropdown.options.Clear();

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                _locales.Add(locale);
                dropdown.options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
            }
        }

        private void OnDropdownChanged(int index)
        {
            if (_suppressCallback || index < 0 || index >= _locales.Count)
                return;

            LocalizationSettings.SelectedLocale = _locales[index];
        }

        private void OnLocaleChanged(Locale locale)
        {
            ApplySelection(locale);
        }

        private void ApplySelection(Locale locale)
        {
            if (locale == null)
                return;

            var idx = _locales.IndexOf(locale);
            if (idx < 0 && _locales.Count > 0)
                idx = 0;

            if (idx >= 0)
            {
                _suppressCallback = true;
                dropdown.SetValueWithoutNotify(idx);
                _suppressCallback = false;
            }
        }
    }
}
