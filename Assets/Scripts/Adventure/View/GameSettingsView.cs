using System;
using UnityEngine;
using UnityEngine.UI;
using Adventure.Settings.ViewModel;
using Zenject;
using UniRx;
using TMPro;

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

        private readonly CompositeDisposable _bindings = new();

        private void Start()
        {
            if (_vm == null)
                throw new System.Exception();
            _vm.IsOpen.Subscribe(OnVisibilityChanged).AddTo(_bindings);

            closeButton.onClick.AddListener(() => _vm.Close());
            exitButton.onClick.AddListener(() => _vm.ExitGame());

            musicSlider.onValueChanged.AddListener(v => _vm.SetMusicVolume( v));
            effectsSlider.onValueChanged.AddListener(v => _vm.SetEffectsVolume (v));
            qualityDropdown.onValueChanged.AddListener(i =>
            {
                _vm.SetQualityLevel(i);
                _vm.ApplyGraphics();
            });
        }

        private void OnVisibilityChanged(bool visible)
        {
            panelRoot.SetActive(visible);
        }
    }
}

