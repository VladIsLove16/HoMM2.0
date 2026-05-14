using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Settings.View
{
    public sealed class AudioSettingsSection : GameSettingsSectionBase
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectsSlider;
        [SerializeField] private Slider voiceSlider;

        public override GameSettingsSectionFeature Feature => GameSettingsSectionFeature.Audio;

        protected override void OnBind()
        {
            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(ViewModel.Audio.MusicVolume);
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }

            if (effectsSlider != null)
            {
                effectsSlider.SetValueWithoutNotify(ViewModel.Audio.SoundsVolume);
                effectsSlider.onValueChanged.AddListener(OnEffectsChanged);
            }
            if (voiceSlider != null)
            {
                voiceSlider.SetValueWithoutNotify(ViewModel.Audio.VoiceVolume);
                voiceSlider.onValueChanged.AddListener(OnEffectsChanged);
            }
        }

        protected override void OnUnbind()
        {
            if (musicSlider != null)
                musicSlider.onValueChanged.RemoveListener(OnMusicChanged);

            if (effectsSlider != null)
                effectsSlider.onValueChanged.RemoveListener(OnEffectsChanged);
        }

        private void OnMusicChanged(float value)
        {
            ViewModel.SetMusicVolume(value);
        }

        private void OnEffectsChanged(float value)
        {
            ViewModel.SetSoundsVolume(value);
        }
        private void OnVoiceChanged(float value)
        {
            ViewModel.SetVoiceVolume(value);
        }
    }
}
