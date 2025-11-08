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
        [SerializeField] private TMP_Dropdown animationSpeedDropdown;

        protected TViewModel ViewModel { get; private set; }
        protected CompositeDisposable Bindings { get; private set; }

        private UnityAction _closeAction;
        private UnityAction _exitAction;

        [Inject]
        public virtual void Construct(TViewModel vm)
        {
            ViewModel = vm ?? throw new ArgumentNullException(nameof(vm));
        }

        protected virtual void Start()
        {
            if (ViewModel == null)
                throw new InvalidOperationException($"{GetType().Name} was not constructed with a ViewModel.");

            Bindings = new CompositeDisposable();

            ViewModel.IsOpen.Subscribe(OnVisibilityChanged).AddTo(Bindings);

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

            ConfigureAnimationControls();
        }

        protected virtual void OnDestroy()
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
            if (animationSpeedDropdown != null)
                animationSpeedDropdown.onValueChanged.RemoveListener(OnAnimationDropdownChanged);

            Bindings?.Dispose();
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

        protected virtual void ConfigureAnimationControls()
        {
            if (animationSpeedDropdown != null)
            {
                animationSpeedDropdown.gameObject.SetActive(false);
            }
        }

        protected void SetupAnimationDropdown(IReadOnlyReactiveProperty<AnimationSpeedMode> source, Action<AnimationSpeedMode> setter, List<string> options)
        {
            if (animationSpeedDropdown == null)
                return;

            animationSpeedDropdown.gameObject.SetActive(true);
            animationSpeedDropdown.ClearOptions();
            if (options != null && options.Count > 0)
            {
                animationSpeedDropdown.AddOptions(options);
            }
            animationSpeedDropdown.SetValueWithoutNotify((int)source.Value);
            animationSpeedDropdown.onValueChanged.AddListener(OnAnimationDropdownChanged);
            source.Subscribe(mode => animationSpeedDropdown.SetValueWithoutNotify((int)mode)).AddTo(Bindings);

            void SetterWrapper(AnimationSpeedMode mode) => setter(mode);
            _animationSetter = SetterWrapper;
        }

        private Action<AnimationSpeedMode> _animationSetter;

        private void OnAnimationDropdownChanged(int index)
        {
            _animationSetter?.Invoke((AnimationSpeedMode)index);
        }
    }
}
