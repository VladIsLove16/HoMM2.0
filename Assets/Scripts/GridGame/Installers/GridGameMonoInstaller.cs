using Adventure.Infrastructure.Persistence;
using Adventure.Infrastructure.State;
using CustomEventBus;
using Game.Achievements;
using Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Adventure.Settings.ViewModel;
using Adventure.Settings.Model;
using UnityEngine.Audio;

public class GridGameMonoInstaller : MonoInstaller
{
    [Header("Gameplay References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private GameNetworkCommandGateway networkCommandGateway;
    [SerializeField] private SceneLoadWatcher sceneLoadWatcher;
    [SerializeField] private AudioMixer audioMixer;
    [Header("View model dependencies")]
    [SerializeField] private GridUnitAssetMap gridUnitAssets;
    [SerializeField] private GridRenderSettingsSO gridRenderSettings;
    [Header("Model dependencies")]
    [SerializeField] private StatusEffectDatas statusEffectDatas;
    [SerializeField] private GameConfigurationService gameConfigurationService;
    [Header("Presentation")]
    [SerializeField] private MonoBehaviour _presentationInstaller;
    public override void InstallBindings()
    {

        BindConfigurationService();
        BindServices();
        BindModels();
        BindViewModels();
        BindGameController();
        BindView();
    }

    private void BindView()
    {
        if (_presentationInstaller is IGamePresentationInstaller gamePresentationInstaller)
            gamePresentationInstaller.Install(Container);
        else
            throw new ArgumentException();
    }

    private void BindConfigurationService()
    {
        Container.BindInterfacesTo<GameConfigurationService>().FromInstance(gameConfigurationService).AsSingle();
    }

    private void BindServices()
    {
        BindPersistenceServices();
        Container.Bind<GameNetworkCommandGateway>().FromInstance(networkCommandGateway).AsSingle();
        Container.Bind<SceneLoadWatcher>().FromInstance(sceneLoadWatcher).AsSingle();
        Container.Bind<IBattleAnimationGate>().To<BattleAnimationGate>().AsSingle();
        Container.Bind<IAnimationSpeedSettings>().To<AnimationSpeedSettings>().AsSingle().WithArguments(AnimationSpeedMode.Fast);
        if (gridRenderSettings == null)
        {
            Debug.LogWarning("[GridGameMonoInstaller] GridRenderSettings is not assigned. Using default settings.");
            var runtimeSettings = ScriptableObject.CreateInstance<GridRenderSettingsSO>();
            Container.Bind<IGridRenderSettings>().FromInstance(runtimeSettings).AsSingle();
        }
        else
        {
            Container.Bind<IGridRenderSettings>().FromInstance(gridRenderSettings).AsSingle();
        }
        Container.Bind<EventBus>().AsSingle();
        Container.Bind<PauseController>().AsSingle();
        Container.Bind<AudioMixer>().AsSingle();
        //BindAchievementServices();
    }

    private void BindPersistenceServices()
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
    //private void BindAchievementServices()
    //{
    //    var catalog = Resources.Load<AchievementCatalog>("Achievements/AchievementCatalog");
    //    if (catalog == null)
    //    {
    //        Debug.LogWarning("AchievementCatalog not found at Resources/Achievements/AchievementCatalog");
    //        catalog = ScriptableObject.CreateInstance<AchievementCatalog>();
    //    }

    //    Container.Bind<AchievementCatalog>().FromInstance(catalog).AsSingle();
    //    Container.Bind<IAchievementDefinitionProvider>().To<AchievementCatalogDefinitionProvider>().AsSingle();
    //    Container.Bind<IAchievementStorage>().To<PlayerPrefsAchievementStorage>().AsSingle();
    //    Container.Bind<IAchievementService>().To<AchievementService>().AsSingle();
    //    Container.Bind<IGameplayEventBus>().To<GameplayEventBus>().AsSingle();
    //    Container.BindInterfacesTo<AchievementEventListener>().AsSingle().NonLazy();
    //}

    private void BindModels()
    {
        Container.Bind<GameSettingsModel>().FromMethod(_ => GameSettingsRuntimeStore.Resolve()).AsSingle();
        Container.BindInterfacesAndSelfTo<GridUnitAssetMap>().FromInstance(gridUnitAssets).AsSingle();
        Container.Bind<StatusEffectDatas>().FromInstance(statusEffectDatas).AsSingle();

        Container.Bind<UnitModelFactory>().AsSingle();
        Container.Bind<MovementSystem>().AsSingle();
        Container.Bind<ActionResolver>().AsSingle();
        Container.Bind<ActionPipeline>().AsSingle();
        Container.Bind<ITurnQueue>().To<TurnQueue>().AsSingle();
        Container.Bind<ITurnService>().To<TurnService>().AsSingle();
        Container.Bind<SpellZoneFactory>().AsSingle();
        Container.Bind<SpellCasterService>().AsSingle();
        Container.Bind<GameModel>().AsSingle().NonLazy();
          if (gameConfigurationService.CurrentGameMode == GameMode.SinglePlayer)
        {
            Container.Bind<IGameCommandExecutor>().To<LocalGameCommandExecutor>().AsSingle();
        }
        else
        {
            Container.Bind<IGameCommandExecutor>().To<NetworkGameCommandExecutor>().AsSingle();
        }
    }

    private void BindViewModels()
    {
        Container.BindInterfacesAndSelfTo<TurnStateViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GameViewModel>().AsSingle().NonLazy();
        Container.Bind<UnitTurnPanelViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GridGameSettingsViewModel>().AsSingle().NonLazy();
        Container.Bind<GridCursorViewModel>().AsSingle().NonLazy();
    }

    private void BindGameController()
    {
        Container.Bind<GameController>().FromInstance(gameController).AsSingle().NonLazy();
    }

}




