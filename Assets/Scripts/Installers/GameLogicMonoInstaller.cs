
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

/// <summary>
/// Installs core gameplay dependencies and delegates presentation-specific wiring to an injected strategy.
/// </summary>
public class GameLogicMonoInstaller : MonoInstaller
{
    [Header("Gameplay References")]
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameNetworkCommandGateway _gameNetworkCommandGateway;
    [SerializeField] private GameUnitDatas _unitDatas;
    [SerializeField] private UnitPrefabManager unitPrefabManager;
    [SerializeField] private SceneLoadWatcher sceneLoadWatcher;
    [SerializeField] private CursorService cursorService;

    [Header("Presentation")]
    [SerializeField] private MonoBehaviour presentationInstallerBehaviour;
    [SerializeField] private SceneGameConfigurationProvider sceneGameConfigurationProvider;

    private IGamePresentationInstaller _presentationInstaller;

    public override void InstallBindings()
    {
        if (presentationInstallerBehaviour == null)
        {
            throw new InvalidOperationException("Presentation installer reference is not set on GameLogicMonoInstaller.");
        }

        _presentationInstaller = presentationInstallerBehaviour as IGamePresentationInstaller;
        if (_presentationInstaller == null)
        {
            throw new InvalidOperationException($"Presentation installer '{presentationInstallerBehaviour.name}' must implement IGamePresentationInstaller.");
        }

        BindServices();
        BindModels();
        BindViewModels();
        BindGameController();
        BindConfigurationProviders();

        _presentationInstaller.Install(Container);
    }

    private void BindServices()
    {
        Container.Bind<GameNetworkCommandGateway>().FromInstance(_gameNetworkCommandGateway).AsSingle();
        Container.Bind<SceneLoadWatcher>().FromInstance(sceneLoadWatcher).AsSingle();
        Container.Bind<UnitPrefabManager>().FromInstance(unitPrefabManager).AsSingle();
        Container.Bind<ICursorService>().FromInstance(cursorService).AsSingle().NonLazy();
    }

    private void BindModels()
    {
        IReadOnlyDictionary<UnitType, UnitDefinitionSO> unitDatasDictionary = _unitDatas.ToDictionary();
        Container.Bind<IReadOnlyDictionary<UnitType, UnitDefinitionSO>>().FromInstance(unitDatasDictionary);

        Container.Bind<UnitModelFactory>().AsSingle().NonLazy();
        Container.Bind<MovementSystem>().AsSingle();
        Container.Bind<ActionResolver>().AsSingle();
        Container.Bind<TurnSystem>().AsSingle();
        Container.BindInterfacesTo<TurnService>().AsSingle();
        Container.Bind<SpellZoneFactory>().AsSingle();
        Container.Bind<SpellCasterService>().AsSingle();
        Container.Bind<GameModel>().AsSingle().NonLazy();
    }

    private void BindViewModels()
    {
        Container.Bind<GameViewModel>().AsSingle().NonLazy();
        Container.BindInterfacesTo<GameViewModel>().FromResolve();
        Container.Bind<UnitTurnPanelViewModel>().AsSingle().NonLazy();
    }

    private void BindGameController()
    {
        if (_gameController == null)
        {
            throw new InvalidOperationException("GameController reference is not assigned.");
        }

        Container.Bind<GameController>().FromInstance(_gameController).AsSingle().NonLazy();
    }

    private void BindConfigurationProviders()
    {
        var configurationService = GameConfigurationService.Instance;
        Container.Bind<GameConfigurationService>().FromInstance(configurationService).AsSingle();
        Container.Bind<SceneGameConfigurationProvider>().FromInstance(sceneGameConfigurationProvider).AsSingle();
        Container.Bind<IGameConfigurationService>().FromResolve().AsSingle();
        Container.Bind<IGameModeProvider>().FromResolve().AsSingle();
        Container.BindInterfacesTo<SceneGameConfigurationProvider>();
    }
}
