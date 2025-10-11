using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Presentation.Mushroom;
using Assets.Scripts.Adventure.Infrastructure.UI;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Adventure.Infrastructure.Input
{
    [DefaultExecutionOrder(-150)]
    public sealed class AdventureInputRouter : MonoBehaviour
    {
        [SerializeField] private AdventurePlayerInput playerInput;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerInteractionController interactionController;
        [SerializeField] private GameSettings settingsMenuBehaviour;
        [SerializeField] private MushroomBookView mushroomBookBehaviour;
        [SerializeField] private AdventureSceneCursorService cursorService;
        private void Awake()
        {
            cursorService.LockToCenter();
        }

        private void OnEnable()
        {
            if (playerInput == null)
                return;

            playerInput.MoveChanged += OnMoveChanged;
            playerInput.LookChanged += OnLookChanged;
            playerInput.SprintChanged += OnSprintChanged;
            playerInput.InteractPerformed += OnInteract;
            playerInput.ShowHintPerformed += OnShowHint;
            playerInput.OpenSettingsPerformed += OnOpenSettings;
            playerInput.OpenMushroomBookPerformed += OnOpenMushroomBook;
        }

        private void OnDisable()
        {
            if (playerInput == null)
                return;

            playerInput.MoveChanged -= OnMoveChanged;
            playerInput.LookChanged -= OnLookChanged;
            playerInput.SprintChanged -= OnSprintChanged;
            playerInput.InteractPerformed -= OnInteract;
            playerInput.ShowHintPerformed -= OnShowHint;
            playerInput.OpenSettingsPerformed -= OnOpenSettings;
            playerInput.OpenMushroomBookPerformed -= OnOpenMushroomBook;

            movementController?.ResetExternalInput();
        }

        private void OnMoveChanged(Vector2 move)
        {
            movementController?.SetMoveInput(move);
            interactionController?.RefreshPrompt();
        }

        private void OnLookChanged(Vector2 delta)
        {
            movementController?.EnqueueLookDelta(delta);
            interactionController?.RefreshPrompt();
        }

        private void OnSprintChanged(bool sprint)
        {
            movementController?.SetSprintInput(sprint);
        }

        private void OnInteract()
        {
            interactionController?.PerformInteract();
        }

        private void OnShowHint()
        {
            interactionController?.DisplayHint();
        }

        private void OnOpenSettings()
        {
            settingsMenuBehaviour?.Open();
        }

        private void OnOpenMushroomBook()
        {
            cursorService.SetCursorState(CursorState.Default);
            cursorService.Unlock();
            mushroomBookBehaviour.Closed += OnMushroomBookClosed;
            mushroomBookBehaviour?.Open();
        }
        private void OnMushroomBookClosed()
        {
            cursorService.LockToCenter(); 
        }
    }
}
