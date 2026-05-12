using Adventure.Infrastructure.Inventory;
using Adventure.Presentation.Interaction;
using Assets.Scripts.Adventure.Infrastructure.Input;
using UnityEngine;
using Zenject;

namespace Adventure.Infrastructure.Interaction
{
    public sealed partial class PlayerInteractionController : MonoBehaviour
    {
        private const string DefaultInteractPrompt = "Interact";
        private const string DefaultCollectPrompt = "Collect";

        [Inject(Optional = true)] private AdventureInput playerInput;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 2f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private InteractionPromptView promptView;
        [SerializeField] private string interactBinding = "LMB";
        //[SerializeField] private string hintBinding = "RMB";
        [SerializeField, Range(0.25f, 5f)] private float hintDisplayDuration = 2f;

        private IInteractable _currentInteractable;
        private bool _hintVisible;
        private float _hintExpiresAt;

        private void Update()
        {
            UpdateCandidate();

            if (_hintVisible && Time.time >= _hintExpiresAt)
            {
                HidePrompt();
            }
        }

        private void OnDisable()
        {
            HidePrompt();
        }

        public void RefreshPrompt()
        {
            UpdateCandidate();
        }

        public void PerformInteract()
        {
            Debug.Log("interaction performed");
            if (!enabled)
            {
                Debug.LogWarning("PlayerInteractionController not enabled!");
                return;
            }

            if (!EnsureCandidate(out var interactable))
            {
                HidePrompt();
                Debug.LogWarning("interactable is not ensured!");
                return;
            }

            var context = new PlayerInteractionContext(transform);

            interactable.Interact(context);
            HidePrompt();
        }

        public void DisplayHint()
        {
            if (!enabled || promptView == null || (playerInput != null))
                return;

            if (!EnsureCandidate(out var candidate))
            {
                HidePrompt();
                return;
            }

            ShowPrompt(candidate, interactBinding);
            _hintVisible = true;
            _hintExpiresAt = Time.time + hintDisplayDuration;
        }

        private void UpdateCandidate()
        {
            if (TryFindInteractable(out var interactable))
            {
                _currentInteractable = interactable;

                ShowPrompt(interactable, interactBinding);
            }
            else
            {
                _currentInteractable = null;
                HidePrompt();
            }
        }

        private bool EnsureCandidate(out IInteractable interactable)
        {
            if (_currentInteractable!=null)
            {
                interactable = _currentInteractable;
                return true;
            }

            if (TryFindInteractable(out interactable))
            {
                _currentInteractable = interactable;
                return true;
            }

            interactable = default;
            return false;
        }

        private void ShowPrompt(IInteractable interactable, string bindingLabel)
        {
            if (promptView == null)
                return;
            if (interactable == null)
                return;

            promptView.Show(bindingLabel, interactable.GetPrompt());
        }

        private void HidePrompt()
        {
            if (!_hintVisible && promptView == null)
                return;

            _hintVisible = false;
            _hintExpiresAt = 0f;
            promptView?.Hide();
        }

        private bool TryFindInteractable(out IInteractable interactable)
        {
            interactable = null;

            if (playerCamera == null)
                return false;

            var origin = playerCamera.transform.position;
            var direction = playerCamera.transform.forward;

            if (!Physics.Raycast(origin, direction, out var hit, interactDistance, interactionMask))
                return false;

            interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable == null)
                return false;

            if (!interactable.CanInteract)
            {
                interactable = null;
                return false;
            }

            return true;
        }
    }
}
