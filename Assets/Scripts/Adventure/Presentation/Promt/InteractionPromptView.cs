using TMPro;
using UnityEngine;

namespace Adventure.Presentation.Interaction
{
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup root;
        [SerializeField] private TextMeshProUGUI bindingLabel;
        [SerializeField] private TextMeshProUGUI actionLabel;

        public void Show(string binding, string action)
        {
            if (bindingLabel != null)
                bindingLabel.text = binding;
            if (actionLabel != null)
                actionLabel.text = action;

            if (root != null)
            {
                root.alpha = 1f;
                root.interactable = false;
                root.blocksRaycasts = false;
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.alpha = 0f;
                root.interactable = false;
                root.blocksRaycasts = false;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
