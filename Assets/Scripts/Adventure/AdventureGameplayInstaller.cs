using Adventure.Application.Dialog;
using Adventure.Domain.Dialog;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Inventory;
using Adventure.Infrastructure.Movement;
using Adventure.Integration.Battle;
using Adventure.Presentation.Dialog;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.Model;
using Adventure.Settings.View;
using Adventure.Settings.ViewModel;
using Assets.Scripts.Adventure.Infrastructure.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public sealed class AdventureGameplayInstaller : MonoInstaller
{
    [Header("Movement")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [Header("Catalogues")]
    [SerializeField] private UnitDefinitionSOCollection mushroomCatalog;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;

    [Header("Battle")]
    [SerializeField] private GridConfigurationGateway gridConfigurationGateway;
    [SerializeField] private ArmyLineupSO playerStartArmy;
    [SerializeField] private int playerFrontlineX = 1;
    [SerializeField] private int enemyFrontlineX = 10;
    [SerializeField] private int rowSpacing = 1;

    [Header("Blocked & Interaction")]
    [SerializeField] private List<CursorStateTexture> cursorStateTextures;
    [SerializeField] private PlayerInteractionController interactionController;
    [SerializeField] private DialogueUIView dialogueUIView;
    [SerializeField] private MushroomBookView mushroomBookView;
    [SerializeField] private GameSettingsView gameSettingsView;
    [Header("Blocked & Interaction")]

    [SerializeField] private AdventureDevTools devTools;
    public override void InstallBindings()
    {
        BindDatabases();
        BindServices();
        BindInputs();
        BindsModels();
        BindsVMS();
        BindViews();
        BindTools();
        
    }

    private void BindsModels()
    {
        Container.Bind<GameSettingsModel>().AsSingle();
        Container.Bind<MushroomInventoryModel>().AsSingle().WithArguments(playerStartArmy.Convert());

    }
    private void BindsVMS()
    {
        Container.BindInterfacesAndSelfTo<MushroomBookViewModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<DialogVM>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameSettingsViewModel>().AsSingle();
        Container.Bind<MenusCoordinatorViewModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<Adventure.Infrastructure.Cursor.CursorViewModel>().AsSingle().NonLazy();
    }

    private void BindViews()
    {
        Container.Bind<DialogueUIView>().FromInstance(dialogueUIView).AsSingle();
        Container.Bind<MushroomBookView>().FromInstance(mushroomBookView).AsSingle();
        Container.Bind<GameSettingsView>().FromInstance(gameSettingsView).AsSingle();
        Container.Bind<NpcDialogueTrigger>()
            .FromComponentsInHierarchy()
            .AsTransient();
        Container.Bind<MushroomCollectible>()
            .FromComponentsInHierarchy()
            .AsTransient();
        Container.Bind<CursorView>().AsSingle().WithArguments(cursorStateTextures).NonLazy();
    }
    private void BindTools()
    {
        Container.Bind<AdventureDevTools>().FromInstance(devTools). AsSingle();
    }
    private void BindServices()
    {
        Container.Bind<PauseController>().AsSingle();
        var resolver = new ArmyFormationResolver(playerFrontlineX, enemyFrontlineX, rowSpacing);
        Container.Bind<ArmyFormationResolver>().FromInstance(resolver).AsSingle(); 
        Container.Bind<BattleLaunchService>().AsSingle();

        if (gridConfigurationGateway != null)
        {
            Container.Bind<IGridConfigurationGateway>().FromInstance(gridConfigurationGateway).AsSingle();
        }
        else
        {
            Debug.LogError("GridConfigurationGateway is not assigned on AdventureGameplayInstaller", this);
        }
    }

    private void BindDatabases()
    {
        Container.Bind<IDialogRepository>().FromInstance(dialogueDatabase).AsSingle();
        Container.Bind<IDialogStateStore>().To<PlayerPrefsDialogStateStore>().AsSingle();
        Container.Bind<UnitDefinitionSOCollection>().FromInstance(mushroomCatalog).AsSingle();
    }

    private void BindInputs()
    {
        Container.Bind<PlayerMovementController>().FromInstance(playerMovementController).AsSingle().NonLazy();
        Container.Bind<PlayerInteractionController>().FromInstance(interactionController).AsSingle().NonLazy();
        Container.Bind<AdventureInput>(). AsSingle().NonLazy();
        Container.Bind<AdventureInputRouter>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<InputModeViewModel>().AsSingle().NonLazy();
    }
}
