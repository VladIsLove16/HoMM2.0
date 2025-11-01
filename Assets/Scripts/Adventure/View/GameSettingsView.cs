using System;
using UnityEngine;
using UnityEngine.UI;
using Adventure.Settings.ViewModel;
using Zenject;
using UniRx;
using TMPro;
using UnityEngine.Audio;

namespace Adventure.Settings.View
{
    public sealed class GameSettingsView : MonoBehaviour
    {
        [Inject] private GameSettingsViewModel _vm;

        [Header("Blocked Elements")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectsSlider;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown animationSpeedDropdown;
        [SerializeField] AudioMixerGroup MusicGroup;
        [SerializeField] AudioMixerGroup SoundsGroup;

        private readonly CompositeDisposable _bindings = new();

        private void Start()
        {
            if (_vm == null)
                throw new System.Exception();
            _vm.IsOpen.Subscribe(OnVisibilityChanged).AddTo(_bindings);

            closeButton.onClick.AddListener(() => _vm.Close());
            exitButton.onClick.AddListener(() => _vm.ExitGame());

            musicSlider.onValueChanged.AddListener(v => OnMusicSliderChanged(v));
            effectsSlider.onValueChanged.AddListener(v => OnSoundsSliderChanged(v));
            qualityDropdown.onValueChanged.AddListener(i =>
            {
                _vm.SetQualityLevel(i);
                _vm.ApplyGraphics();
            });
        }

        private void OnMusicSliderChanged(float v)
        {
            _vm.SetMusicVolume(v);
        }
        private void OnSoundsSliderChanged(float v)
        {
            _vm.SetSoundsVolume(v);
        }

        private void OnVisibilityChanged(bool visible)
        {
            UnityLogger.Log("GameSettingsView visibility changed: " + visible);    
            panelRoot.SetActive(visible);
        }
    }
}

