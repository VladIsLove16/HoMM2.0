using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Shared.Localization
{
    [DisallowMultipleComponent]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private LocalizedString localizedString;
        [SerializeField] private TMP_Text tmpText;
        [SerializeField] private Text uiText;

        private void Awake()
        {
            if (tmpText == null)
                tmpText = GetComponent<TMP_Text>();

            if (uiText == null)
                uiText = GetComponent<Text>();
        }

        private void OnEnable()
        {
            LocalizationSettings.InitializationOperation.WaitForCompletion();
            localizedString.StringChanged += OnValueChanged;
            localizedString.RefreshString();
        }

        private void OnDisable()
        {
            localizedString.StringChanged -= OnValueChanged;
        }

        private void OnValueChanged(string value)
        {
            if (tmpText != null)
            {
                tmpText.text = value;
            }

            if (uiText != null)
            {
                uiText.text = value;
            }
        }

        public void Refresh()
        {
            localizedString.RefreshString();
        }
    }
}
