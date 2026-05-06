using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SharedView.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class ButtonAudioFeedback : MonoBehaviour, IPointerEnterHandler
    {
        private IGameAudioService _audio;
        private Button _button;
        private bool _clickSubscribed;

        public void Construct(IGameAudioService audio)
        {
            _audio = audio;
            EnsureButton();
            RegisterClick();
        }

        private void OnEnable()
        {
            EnsureButton();
            RegisterClick();
        }

        private void OnDisable()
        {
            UnregisterClick();
        }

        private void OnDestroy()
        {
            UnregisterClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsInteractable())
            {
                _audio?.PlayButtonHover();
            }
        }

        private void HandleClick()
        {
            if (IsInteractable())
            {
                _audio?.PlayButtonClick();
            }
        }

        private void EnsureButton()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        private void RegisterClick()
        {
            if (_clickSubscribed || _button == null)
                return;

            _button.onClick.AddListener(HandleClick);
            _clickSubscribed = true;
        }

        private void UnregisterClick()
        {
            if (!_clickSubscribed || _button == null)
                return;

            _button.onClick.RemoveListener(HandleClick);
            _clickSubscribed = false;
        }

        private bool IsInteractable()
        {
            EnsureButton();

            return _button != null && _button.IsInteractable();
        }
    }
}
