using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Players;
using Adventure.Application.VMs;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using System;
using UnityEngine;
using Zenject;

public sealed class AdventureInputRouter : IDisposable
{
    [Inject] private AdventureInput adventureCharacterInput;
    [Inject] private ILocalAdventurePlayerProvider localPlayerProvider;

    [Inject] private IInputModeVM inputModeVM;
    [Inject] private AdventureMenusCoordinatorViewModel menusVM;
    [Inject] private AdventureGameSettingsViewModel settingsVM;
    [Inject] private MushroomBookViewModel bookVM;
    [InjectOptional] private HelpMenuViewModel helpMenuVM;

    [Inject]
    private void Construct()
    {
        adventureCharacterInput.MoveChanged += OnMoveChanged;
        adventureCharacterInput.LookChanged += OnLookChanged;
        adventureCharacterInput.SprintChanged += OnSprintChanged;
        adventureCharacterInput.InteractPerformed += OnInteract;
        adventureCharacterInput.OpenSettingsPerformed += OnOpenSettings;
        adventureCharacterInput.OpenMushroomBookPerformed += OnOpenMushroomBook;
        adventureCharacterInput.OpenHelpMenuPerformed += OnOpenHelpMenu;
        inputModeVM.OnModeChanged += OnInputModeChanged;
        localPlayerProvider.PlayerChanged += OnPlayerChanged;
        ApplyCurrentInputMode();
    }

    public void Dispose()
    {
        adventureCharacterInput.MoveChanged -= OnMoveChanged;
        adventureCharacterInput.LookChanged -= OnLookChanged;
        adventureCharacterInput.SprintChanged -= OnSprintChanged;
        adventureCharacterInput.InteractPerformed -= OnInteract;
        adventureCharacterInput.OpenSettingsPerformed -= OnOpenSettings;
        adventureCharacterInput.OpenMushroomBookPerformed -= OnOpenMushroomBook;
        adventureCharacterInput.OpenHelpMenuPerformed -= OnOpenHelpMenu;
        inputModeVM.OnModeChanged -= OnInputModeChanged;
        localPlayerProvider.PlayerChanged -= OnPlayerChanged;
    }

    private void OnMoveChanged(Vector2 move)
    {
        var movementController = localPlayerProvider.MovementController;
        if (movementController == null)
            return;

        if (inputModeVM.CanMove)
        {
            movementController.SetMoveInput(move);
            return;
        }

        movementController.ClearMoveInput();
    }

    private void OnLookChanged(Vector2 delta)
    {
        var movementController = localPlayerProvider.MovementController;
        if (movementController == null)
            return;

        if (inputModeVM.CanLook)
        {
            movementController.EnqueueLookDelta(delta);
            return;
        }

        movementController.ClearLookInput();
    }

    private void OnSprintChanged(bool sprint)
    {
        var movementController = localPlayerProvider.MovementController;
        if (movementController == null)
            return;

        if (inputModeVM.CanMove)
        {
            movementController.SetSprintInput(sprint);
            return;
        }

        movementController.SetSprintInput(false);
    }

    private void OnInteract()
    {
        if (!inputModeVM.CanMove)
            return;

        var interactionController = localPlayerProvider.InteractionController;
        if (interactionController == null)
            return;

        interactionController.PerformInteract();
    }

    private void OnOpenSettings()
    {
        UnityLogger.Log("OnOpenSettings clicked");
        if (menusVM.HasAnyOpen)
            menusVM.CloseTopmost();
        else
            settingsVM.Toggle();
    }
    private void OnOpenHelpMenu()
    {
        if(helpMenuVM == null)
        {
            UnityLogger.Log("HelpMenuViewModel is not injected in AdventureInputRouter");
            return;
        }
        helpMenuVM.Toggle();
    }

    private void OnOpenMushroomBook() => bookVM.Toggle();

    private void OnInputModeChanged(InputMode _)
    {
        ApplyCurrentInputMode();
    }

    private void OnPlayerChanged()
    {
        ApplyCurrentInputMode();
    }

    private void ApplyCurrentInputMode()
    {
        var movementController = localPlayerProvider.MovementController;
        if (movementController == null)
            return;

        if (!inputModeVM.CanMove)
        {
            movementController.ClearMoveInput();
        }

        if (!inputModeVM.CanLook)
        {
            movementController.ClearLookInput();
        }
    }
}
