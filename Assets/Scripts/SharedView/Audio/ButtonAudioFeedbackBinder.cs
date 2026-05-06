using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SharedView.Audio
{
    public sealed class ButtonAudioFeedbackBinder : ITickable, IDisposable
    {
        private readonly IGameAudioService _audio;
        private readonly HashSet<Button> _boundButtons = new();
        private float _nextScanTime;

        public ButtonAudioFeedbackBinder(IGameAudioService audio)
        {
            _audio = audio;
        }

        public void Tick()
        {
            if (Time.unscaledTime < _nextScanTime)
                return;

            _nextScanTime = Time.unscaledTime + 0.5f;
            BindAllActiveButtons();
        }

        public void Dispose()
        {
            _boundButtons.Clear();
        }

        private void BindAllActiveButtons()
        {
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var button in buttons)
            {
                if (button == null || !_boundButtons.Add(button))
                    continue;

                var feedback = button.GetComponent<ButtonAudioFeedback>();
                if (feedback == null)
                {
                    feedback = button.gameObject.AddComponent<ButtonAudioFeedback>();
                }

                feedback.Construct(_audio);
            }
        }
    }
}
