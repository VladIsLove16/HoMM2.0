using Adventure.Application.Dialog;
using Adventure.Domain.Dialog;
using Adventure.Domain.Inventory;
using Adventure.Infrastructure.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Inventory;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Persistence;
using Adventure.Infrastructure.State;
using Adventure.Integration.Battle;
using Adventure.Presentation.Dialog;
using Adventure.Presentation.Mushroom;
using Adventure.Settings.Model;
using Adventure.Settings.View;
using Adventure.Settings.ViewModel;
using Game.Events;
using Assets.Scripts.Adventure.Infrastructure.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Audio;
using CustomEventBus;

public sealed class AdventureGameplayInstaller : MonoInstaller
{
    [Header("Movement")]
    [SerializeField] private PlayerMovementController playerMovementController;
    [Header("Catalogues")]
    [SerializeField] private AdventureMushroomAssetMap mushroomAssetMap;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;

    [Header("Battle")]
    [SerializeField] private GridConfigurationGateway gridConfigurationGateway;
    [SerializeField] private ArmyLineupSO playerStartArmy;
    [SerializeField] private int playerFrontlineY = 1;
    [SerializeField] private int enemyFrontlineY = 10;
    [SerializeField] private int columnSpacing = 1;

    [Header("Blocked & Interaction")]
    [SerializeField] private List<CursorStateTexture> cursorStateTextures;
    [SerializeField] private PlayerInteractionController interactionController;
    [SerializeField] private DialogueUIView dialogueUIView;
    [SerializeField] private MushroomBookView mushroomBookView;
    [SerializeField] private AdventureGameSettingsView gameSettingsView;
    [SerializeField] private HelpMenu helpMenu;
    [SerializeField] private AudioMixer audioMixer;
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
        Container.Bind<GameSettingsModel>().FromMethod(_ => GameSettingsRuntimeStore.Resolve()).AsSingle();
        Container.Bind<MushroomInventoryModel>().AsSingle().WithArguments(playerStartArmy.Convert());
        Container.Bind<AudioMixer>().FromInstance(audioMixer).AsSingle().NonLazy();
    }
    private void BindsVMS()
    {
        Container.BindInterfacesAndSelfTo<MushroomBookViewModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<DialogVM>().AsSingle();
        Container.BindInterfacesAndSelfTo<AdventureGameSettingsViewModel>().AsSingle().NonLazy();
        Container.Bind<AdventureMenusCoordinatorViewModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<Adventure.Infrastructure.Cursor.CursorViewModel>().AsSingle().NonLazy();
    }

    private void BindViews()
    {
        Container.Bind<MushroomBookView>().FromInstance(mushroomBookView).AsSingle();
        Container.Bind<DialogueUIView>().FromInstance(dialogueUIView).AsSingle();
        Container.Bind<AdventureGameSettingsView>().FromInstance(gameSettingsView).AsSingle().NonLazy();
        Container.Bind<NpcDialogueTrigger>()
            .FromComponentsInHierarchy()
            .AsTransient();
        Container.Bind<MushroomCollectible>()
            .FromComponentsInHierarchy()
            .AsTransient();
        Container.Bind<CursorView>().AsSingle().WithArguments(cursorStateTextures).NonLazy();
        Container.BindInterfacesAndSelfTo<HelpMenu>().FromInstance(helpMenu).AsSingle().NonLazy();
    }
    private void BindTools()
    {
        Container.Bind<AdventureDevTools>().FromInstance(devTools).AsSingle().NonLazy();
    }
    private void BindServices()
    {
        BindPersistence();
        Container.Bind<PauseController>().AsSingle();
        var resolver = new ArmyFormationResolver(playerFrontlineY, enemyFrontlineY, columnSpacing);
        Container.Bind<ArmyFormationResolver>().FromInstance(resolver).AsSingle(); 
        Container.Bind<BattleLaunchService>().AsSingle();
        Container.BindInterfacesTo<AdventureStateBootstrap>().AsSingle().NonLazy();

        if (gridConfigurationGateway != null)
        {
            Container.Bind<IGridConfigurationGateway>().FromInstance(gridConfigurationGateway).AsSingle();
        }
        else
        {
            Debug.LogError("GridConfigurationGateway is not assigned on AdventureGameplayInstaller", this);
        }
        Container.Bind<EventBus>().AsSingle();
    }

    private void BindPersistence()
    {
        if (!Container.HasBinding<IJsonFileStorage>())
        {
            Container.Bind<IJsonFileStorage>().To<JsonFileStorage>().AsSingle();
        }

        if (!Container.HasBinding<IDataRepository<GameSettingsSaveData>>())
        {
            Container.Bind<IDataRepository<GameSettingsSaveData>>()
                .To<JsonDataRepository<GameSettingsSaveData>>()
                .AsSingle()
                .WithArguments("game-settings");
        }

        if (!Container.HasBinding<IDataRepository<GameStateSaveData>>())
        {
            Container.Bind<IDataRepository<GameStateSaveData>>()
                .To<JsonDataRepository<GameStateSaveData>>()
                .AsSingle()
                .WithArguments("game-state");
        }

        if (!Container.HasBinding<AdventureStatePersistenceInitializer>())
        {
            Container.BindInterfacesTo<AdventureStatePersistenceInitializer>().AsSingle().NonLazy();
        }
    }

    private void BindDatabases()
    {
        Container.Bind<IDialogRepository>().FromInstance(dialogueDatabase).AsSingle();
        Container.Bind<IDialogStateStore>().To<PlayerPrefsDialogStateStore>().AsSingle();
        Container.Bind<IUnitStatsProvider>().FromInstance(mushroomAssetMap).AsSingle();
        Container.Bind<IUnitViewDefinition<MushroomCollectible>>().FromInstance(mushroomAssetMap).AsSingle();
        Container.Bind<AdventureMushroomAssetMap>().FromInstance(mushroomAssetMap).AsSingle();
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
