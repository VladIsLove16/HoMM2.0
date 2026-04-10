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
            EnsureVisible();

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
        }

        public void Hide()
        {
            if (root != null)
            {
                root.alpha = 0f;
                root.interactable = false;
                root.blocksRaycasts = false;

                if (root.gameObject != gameObject)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void EnsureVisible()
        {
            if (root != null && !root.gameObject.activeSelf)
            {
                root.gameObject.SetActive(true);
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }
}
