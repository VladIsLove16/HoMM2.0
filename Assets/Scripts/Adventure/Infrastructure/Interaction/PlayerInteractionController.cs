using Adventure.Application.Inventory;
using Adventure.Infrastructure.Inventory;
using Adventure.Presentation.Interaction;
using Assets.Scripts.Adventure.Infrastructure.Input;
using UnityEngine;

namespace Adventure.Infrastructure.Interaction
{
    public sealed partial class PlayerInteractionController : MonoBehaviour
    {
        private const string DefaultInteractPrompt = "Interact";
        private const string DefaultCollectPrompt = "Collect";

        [SerializeField] private AdventurePlayerInput playerInput;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 2f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private InteractionPromptView promptView;
        [SerializeField] private string interactBinding = "LMB";
        [SerializeField] private string hintBinding = "RMB";
        [SerializeField, Range(0.25f, 5f)] private float hintDisplayDuration = 2f;

        private InteractionCandidate? _currentCandidate;
        private bool _hintVisible;
        private float _hintExpiresAt;

        private void Awake()
        {
            if (playerInput == null)
                playerInput = GetComponent<AdventurePlayerInput>();
        }

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
            if (!enabled || (playerInput != null && !playerInput.isActiveAndEnabled))
                return;

            if (!EnsureCandidate(out var candidate))
            {
                HidePrompt();
                return;
            }

            var context = new PlayerInteractionContext(transform);

            if (candidate.Collectible != null)
            {
                if (candidate.Collectible is IMushroomCollectible mushroom)
                {
                    candidate.Collectible.Collect(context);
                }

                HidePrompt();
                return;
            }

            candidate.Interactable?.Interact(context);
            HidePrompt();
        }

        public void DisplayHint()
        {
            if (!enabled || promptView == null || (playerInput != null && !playerInput.isActiveAndEnabled))
                return;

            if (!EnsureCandidate(out var candidate))
            {
                HidePrompt();
                return;
            }

            ShowPrompt(candidate, hintBinding);
            _hintVisible = true;
            _hintExpiresAt = Time.time + hintDisplayDuration;
        }

        private void UpdateCandidate()
        {
            if (TryFindCandidate(out var candidate))
            {
                _currentCandidate = candidate;

                var binding = _hintVisible ? hintBinding : interactBinding;
                ShowPrompt(candidate, binding);
            }
            else
            {
                _currentCandidate = null;
                HidePrompt();
            }
        }

        private bool EnsureCandidate(out InteractionCandidate candidate)
        {
            if (_currentCandidate.HasValue)
            {
                candidate = _currentCandidate.Value;
                return true;
            }

            if (TryFindCandidate(out candidate))
            {
                _currentCandidate = candidate;
                return true;
            }

            candidate = default;
            return false;
        }

        private void ShowPrompt(InteractionCandidate candidate, string bindingLabel)
        {
            if (promptView == null)
                return;

            var prompt = string.IsNullOrWhiteSpace(candidate.Prompt)
                ? (candidate.Type == InteractionType.Collectible ? DefaultCollectPrompt : DefaultInteractPrompt)
                : candidate.Prompt;

            promptView.Show(bindingLabel, prompt);
        }

        private void HidePrompt()
        {
            if (!_hintVisible && promptView == null)
                return;

            _hintVisible = false;
            _hintExpiresAt = 0f;
            promptView?.Hide();
        }

        private bool TryFindCandidate(out InteractionCandidate candidate)
        {
            candidate = default;

            if (playerCamera == null)
                return false;

            var origin = playerCamera.transform.position;
            var direction = playerCamera.transform.forward;

            if (!Physics.Raycast(origin, direction, out var hit, interactDistance, interactionMask))
                return false;

            var collectible = hit.collider.GetComponentInParent<ICollectible>();
            var interactable = collectible ?? hit.collider.GetComponentInParent<IInteractable>();

            if (interactable == null)
                return false;

            var prompt = interactable.GetPrompt();
            var type = collectible != null ? InteractionType.Collectible : InteractionType.Generic;

            candidate = new InteractionCandidate(type, interactable, collectible, prompt);
            return true;
        }
    }
}
