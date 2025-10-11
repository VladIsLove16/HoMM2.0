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
    [SerializeField] private ArmyLineupSO defaultLineup;
    [SerializeField] private int playerFrontlineX = 1;
    [SerializeField] private int enemyFrontlineX = 10;
    [SerializeField] private int rowSpacing = 1;

    [Header("Blocked & Interaction")]
    [SerializeField] private List<CursorStateTexture> cursorStateTextures;
    [SerializeField] private PlayerInteractionController interactionController;
    [SerializeField] private DialogueUIView dialogueUIView;
    [SerializeField] private MushroomBookView mushroomBookView;
    [SerializeField] private GameSettingsView gameSettings;

    public override void InstallBindings()
    {
        BindDatabases();
        BindServices();
        BindInputs();
        BindsModels();
        BindsVMS();
        BindViews();
        
    }

    
    private void BindViews()
    {
        Container.Bind<DialogueUIView>().FromInstance(dialogueUIView).AsSingle();
        Container.Bind<MushroomBookView>().FromInstance(mushroomBookView).AsSingle();
        Container.Bind<NpcDialogueTrigger>()
            .FromComponentsInHierarchy()
            .AsTransient();
        Container.Bind<MushroomCollectible>()
            .FromComponentsInHierarchy()
            .AsTransient();
    }

    private void BindsVMS()
    {
        Container.Bind<MushroomBookViewModel>().AsSingle();
        Container.Bind<DialogVM>().AsSingle();
        Container.Bind<MushroomCollectionViewModel>().AsSingle();
        Container.Bind<GameSettingsViewModel>().AsSingle();
        Container.Bind<MenusCoordinator>().AsSingle();

    }
    private void BindsModels()
    {
        Container.Bind<GameSettingsModel>().AsSingle();
    }

    private void BindServices()
    {
        Container.Bind<PauseController>().AsSingle();
        Container.Bind<IDialogRepository>().FromInstance(dialogueDatabase).AsSingle();
        var resolver = new ArmyFormationResolver(playerFrontlineX, enemyFrontlineX, rowSpacing);
        Container.Bind<ArmyFormationResolver>().FromInstance(resolver).AsSingle(); Container.Bind<MushroomInventoryModel>().AsSingle();
        Container.Bind<BattleLaunchService>().AsSingle();
        Container.Bind<BattlePreparationService>().AsSingle();

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
        Container.Bind<IDialogStateStore>().To<PlayerPrefsDialogStateStore>().AsSingle();
        Container.Bind<UnitDefinitionSOCollection>().FromInstance(mushroomCatalog).AsSingle();
    }

    private void BindInputs()
    {
        Container.Bind<ICursorService>().To<AdventureSceneCursorService>().AsSingle().WithArguments(cursorStateTextures);
        Container.Bind<ICursorContextManager>().To<CursorContextManager>().AsSingle();
        Container.Bind<IInputModeService>().To<InputModeViewModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<InputCoordinator>().AsSingle();
    }
}
