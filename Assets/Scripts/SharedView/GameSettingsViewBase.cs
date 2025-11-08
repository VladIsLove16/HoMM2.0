using System;
using System.Collections.Generic;
using Adventure.Settings.ViewModel;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Adventure.Settings.View
{
    public abstract class GameSettingsViewBase<TViewModel> : MonoBehaviour where TViewModel : GameSettingsViewModel
    {
        [Header("Common")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectsSlider;
        [SerializeField] private TMP_Dropdown qualityDropdown;

        protected TViewModel ViewModel { get; private set; }
        protected CompositeDisposable Bindings { get; private set; }

        private UnityAction _closeAction;
        private UnityAction _exitAction;
        private bool _initialized;

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

                if (musicSlider != null)
                    musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
                if (effectsSlider != null)
                    effectsSlider.onValueChanged.RemoveListener(OnEffectsSliderChanged);
                if (qualityDropdown != null)
                    qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);

                Bindings?.Dispose();
            }
        }

        private void OnQualityChanged(int value)
        {
            ViewModel.SetQualityLevel(value);
            ViewModel.ApplyGraphics();
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

        }
    }
}
