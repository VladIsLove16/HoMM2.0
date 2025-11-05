using Game.Events;
using Adventure.Infrastructure.Persistence;
using Adventure.Infrastructure.State;
using Game.Achievements;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GridGameMonoInstaller : MonoInstaller
{
    [Header("Gameplay References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private GameNetworkCommandGateway networkCommandGateway;
    [SerializeField] private UnitDefinitionSOCollection unitDefinitionSOCollection;
    [SerializeField] private StatusEffectDatas statusEffectDatas;
    [SerializeField] private UnitPrefabManager unitPrefabManager;
    [SerializeField] private SceneLoadWatcher sceneLoadWatcher;
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
        Container.Bind<UnitPrefabManager>().FromInstance(unitPrefabManager).AsSingle();
        Container.Bind<IBattleAnimationGate>().To<BattleAnimationGate>().AsSingle();
        Container.Bind<IAnimationSpeedSettings>().To<AnimationSpeedSettings>().AsSingle();
        BindAchievementServices();
    }

    private void BindPersistenceServices()
    {
        Container.Bind<IJsonFileStorage>().To<JsonFileStorage>().AsSingle().IfNotBound();
        Container.Bind<IDataRepository<GameSettingsSaveData>>()
            .To<JsonDataRepository<GameSettingsSaveData>>()
            .AsSingle()
            .IfNotBound()
            .WithArguments("game-settings");
        Container.Bind<IDataRepository<GameStateSaveData>>()
            .To<JsonDataRepository<GameStateSaveData>>()
            .AsSingle()
            .IfNotBound()
            .WithArguments("game-state");
        Container.BindInterfacesTo<AdventureStatePersistenceInitializer>().AsSingle().IfNotBound().NonLazy();
    }
    private void BindAchievementServices()
    {
        var catalog = Resources.Load<AchievementCatalog>("Achievements/AchievementCatalog");
        if (catalog == null)
        {
            Debug.LogWarning("AchievementCatalog not found at Resources/Achievements/AchievementCatalog");
            catalog = ScriptableObject.CreateInstance<AchievementCatalog>();
        }

        Container.Bind<AchievementCatalog>().FromInstance(catalog).AsSingle();
        Container.Bind<IAchievementDefinitionProvider>().To<AchievementCatalogDefinitionProvider>().AsSingle();
        Container.Bind<IAchievementStorage>().To<PlayerPrefsAchievementStorage>().AsSingle();
        Container.Bind<IAchievementService>().To<AchievementService>().AsSingle();
        Container.Bind<IGameplayEventBus>().To<GameplayEventBus>().AsSingle();
        Container.BindInterfacesTo<AchievementEventListener>().AsSingle().NonLazy();
    }

    private void BindModels()
    {
        IReadOnlyDictionary<UnitType, UnitDefinitionSO> unitDatasDictionary = unitDefinitionSOCollection.ToDictionary();
        Container.Bind<IReadOnlyDictionary<UnitType, UnitDefinitionSO>>().FromInstance(unitDatasDictionary);

        Container.Bind<UnitModelFactory>().AsSingle().NonLazy();
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
        Container.BindInterfacesTo<TurnStateViewModel>().AsSingle().NonLazy();
        Container.Bind<GameViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesTo<GameViewModel>().FromResolve();
        Container.Bind<UnitTurnPanelViewModel>().AsSingle().NonLazy();
        Container.Bind<GridCursorViewModel>().AsSingle().NonLazy();
    }

    private void BindGameController()
    {
        Container.Bind<GameController>().FromInstance(gameController).AsSingle().NonLazy();
    }
}




