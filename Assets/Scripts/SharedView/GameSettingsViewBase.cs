using System;
using System.Collections.Generic;
using Adventure.Settings.ViewModel;
//using Shared.Localization;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;
using Game.Achievements;

namespace Adventure.Settings.View
{
    public abstract class GameSettingsViewBase<TViewModel> : MonoBehaviour where TViewModel : GameSettingsViewModel
    {
        [Header("Common")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectsSlider;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown languageDropdown;

        protected TViewModel ViewModel { get; private set; }
        protected CompositeDisposable Bindings { get; private set; }

        private UnityAction _closeAction;
        private UnityAction _exitAction;
        private UnityAction _achievementsAction;
        private bool _initialized;
        private bool _waitingLocalizationInit;
        private bool _achievementsBound;

        [InjectOptional] private AchievementsViewModel _achievementsViewModel;

        [Inject]
        public virtual void Construct(TViewModel vm)
        {
            ViewModel = vm ?? throw new ArgumentNullException(nameof(vm));
            TryInitialize();
        }

        protected virtual void Start()
        {
            TryInitialize();
        }

        protected virtual void OnDestroy()
        {
            if (_initialized)
            {
                if (closeButton != null && _closeAction != null)
                    closeButton.onClick.RemoveListener(_closeAction);
                if (exitButton != null && _exitAction != null)
                    exitButton.onClick.RemoveListener(_exitAction);
                if (achievementsButton != null && _achievementsAction != null)
                    achievementsButton.onClick.RemoveListener(_achievementsAction);

                if (musicSlider != null)
                    musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
                if (effectsSlider != null)
                    effectsSlider.onValueChanged.RemoveListener(OnEffectsSliderChanged);
                if (qualityDropdown != null)
                    qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
                if (languageDropdown != null)
                    languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);

                if (_waitingLocalizationInit)
                {
                    LocalizationSettings.InitializationOperation.Completed -= OnLocalizationInitializationCompleted;
                    _waitingLocalizationInit = false;
                }

                Bindings?.Dispose();
            }
        }

        private void OnQualityChanged(int value)
        {
            ViewModel.SetQualityLevel(value);
            ViewModel.ApplyGraphics();
        }
        private void OnLanguageChanged(int value)
        {
            ViewModel.LanguageIndex.SetValueAndForceNotify(value);
        }

        private void OnMusicSliderChanged(float value)
        {
            ViewModel.SetMusicVolume(value);
        }

        private void OnEffectsSliderChanged(float value)
        {
            ViewModel.SetSoundsVolume(value);
        }

        private void OnVisibilityChanged(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }
        }





        public virtual void TryInitialize()
        {
            if (_initialized || ViewModel == null)
                return;

            _initialized = true;
            Bindings = new CompositeDisposable();

            ViewModel.IsOpen.Subscribe(OnVisibilityChanged).AddTo(Bindings);
            OnVisibilityChanged(ViewModel.IsOpen.Value);

            if (closeButton != null)
            {
                _closeAction = () => ViewModel.Close();
                closeButton.onClick.AddListener(_closeAction);
            }

            if (exitButton != null)
            {
                _exitAction = () => ViewModel.ExitGame();
                exitButton.onClick.AddListener(_exitAction);
            }

            BindAchievementsButton();

            if (musicSlider != null)
            {
                musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
                musicSlider.SetValueWithoutNotify(ViewModel.Audio.MusicVolume);
            }

            if (effectsSlider != null)
            {
                effectsSlider.onValueChanged.AddListener(OnEffectsSliderChanged);
                effectsSlider.SetValueWithoutNotify(ViewModel.Audio.EffectsVolume);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
                qualityDropdown.SetValueWithoutNotify(ViewModel.Graphics.QualityLevel);
            }
            if (languageDropdown != null)
            {
                ConfigureLanguageDropdown();
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
                ViewModel.LanguageIndex.Subscribe(idx =>
                    languageDropdown.SetValueWithoutNotify(idx)).AddTo(Bindings);
            }
        }

        private void BindAchievementsButton()
        {
            if (_achievementsBound || _achievementsViewModel == null)
                return;

            if (achievementsButton == null)
                return;

            achievementsButton.onClick.RemoveAllListeners();
            _achievementsAction = () => _achievementsViewModel.Toggle();
            achievementsButton.onClick.AddListener(_achievementsAction);
            UpdateAchievementsLabel();
            _achievementsBound = true;
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

        private void UpdateAchievementsLabel()
        {
            if (achievementsButton == null)
                return;

            var textComponent = achievementsButton.GetComponentInChildren<TMP_Text>();
            if (textComponent == null)
                return;
        }
    }
}
