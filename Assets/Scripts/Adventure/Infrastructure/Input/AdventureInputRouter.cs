using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using UnityEngine;
using Zenject;

[DefaultExecutionOrder(-150)]
public sealed class AdventureInputRouter : MonoBehaviour
{
    [SerializeField] private AdventurePlayerInput playerInput;
    [SerializeField] private PlayerMovementController movementController;
    [SerializeField] private PlayerInteractionController interactionController;

    [Inject] private InputModeViewModel inputModeVM;
    [Inject] private MenusCoordinatorViewModel menusVM;
    [Inject] private GameSettingsViewModel settingsVM;
    [Inject] private MushroomBookViewModel bookVM;

    private void OnEnable()
    {
        playerInput.MoveChanged += OnMoveChanged;
        playerInput.LookChanged += OnLookChanged;
        playerInput.SprintChanged += OnSprintChanged;
        playerInput.InteractPerformed += OnInteract;
        playerInput.OpenSettingsPerformed += OnOpenSettings;
        playerInput.OpenMushroomBookPerformed += OnOpenMushroomBook;
    }

    private void OnDisable()
    {
        playerInput.MoveChanged -= OnMoveChanged;
        playerInput.LookChanged -= OnLookChanged;
        playerInput.SprintChanged -= OnSprintChanged;
        playerInput.InteractPerformed -= OnInteract;
        playerInput.OpenSettingsPerformed -= OnOpenSettings;
        playerInput.OpenMushroomBookPerformed -= OnOpenMushroomBook;
    }

    private void OnMoveChanged(Vector2 move)
    {
        if (inputModeVM.CanMove)
            movementController?.SetMoveInput(move);
    }

    private void OnLookChanged(Vector2 delta)
    {
        if (inputModeVM.CanLook)
            movementController?.EnqueueLookDelta(delta);
    }

    private void OnSprintChanged(bool sprint)
    {
        if (inputModeVM.CanMove)
            movementController?.SetSprintInput(sprint);
    }

    private void OnInteract() => interactionController?.PerformInteract();

    private void OnOpenSettings()
    {
        if (menusVM.HasAnyOpen)
            menusVM.CloseFirst();
        else
            settingsVM.Toggle();
    }

    private void OnOpenMushroomBook() => bookVM.Toggle();
}
