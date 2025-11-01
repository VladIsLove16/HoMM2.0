using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using System;
using UnityEngine;
using Zenject;

[DefaultExecutionOrder(-150)]
public sealed class AdventureInputRouter : IDisposable
{
    [Inject] private AdventureInput adventureCharacterInput;
    [Inject] private PlayerMovementController movementController;
    [Inject] private PlayerInteractionController interactionController;

    [Inject] private IInputModeVM inputModeVM;
    [Inject] private MenusCoordinatorViewModel menusVM;
    [Inject] private GameSettingsViewModel settingsVM;
    [Inject] private MushroomBookViewModel bookVM;
    [Inject] private HelpMenu helpMenu;
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
    }

   

    public void Dispose()
    {
        adventureCharacterInput.MoveChanged -= OnMoveChanged;
        adventureCharacterInput.LookChanged -= OnLookChanged;
        adventureCharacterInput.SprintChanged -= OnSprintChanged;
        adventureCharacterInput.InteractPerformed -= OnInteract;
        adventureCharacterInput.OpenSettingsPerformed -= OnOpenSettings;
        adventureCharacterInput.OpenMushroomBookPerformed -= OnOpenMushroomBook;
    }

    private void OnMoveChanged(Vector2 move)
    {
        if (inputModeVM.CanMove)
            movementController?.SetMoveInput(move);
        else
            movementController?.SetMoveInput(Vector2.zero);
    }

    private void OnLookChanged(Vector2 delta)
    {
        if (inputModeVM.CanLook)
        {
            movementController?.EnqueueLookDelta(delta);
        }
    }

    private void OnSprintChanged(bool sprint)
    {
        if (inputModeVM.CanMove)
            movementController?.SetSprintInput(sprint);
    }

    private void OnInteract()
    {
        if (inputModeVM.CanMove)
            interactionController?.PerformInteract();
    }

    private void OnOpenSettings()
    {
        if (menusVM.HasAnyOpen)
            menusVM.CloseFirst();
        else
            settingsVM.Toggle();
    }
    private void OnOpenHelpMenu()
    {
        helpMenu.Toggle();
    }
    private void OnOpenMushroomBook() => bookVM.Toggle();
}
