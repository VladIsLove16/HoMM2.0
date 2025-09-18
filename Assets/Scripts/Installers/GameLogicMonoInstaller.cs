using Game.Network;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Zenject;
public enum GridRenderStrategy { PerCell, Single }

public class GameLogicMonoInstaller : MonoInstaller
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameNetworkCommandGateway _gameNetworkCommandGateway;
    [SerializeField] private GameView3D _gameView3D;
    [SerializeField] private AttackActionPanel _attackActionPanel;
    [SerializeField] private UnitTurnPanelView _MVVMUnitTurnPanel;
    [SerializeField] private InGameUI _inGameUI;
    [SerializeField] private UnitStatsPanel _unitStatsPanel;
    [SerializeField] private GridView _gridView;
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private GameObject singleGridPrefab;
    [SerializeField] private UnitDefinitionSO[] _unitDatas;
    [SerializeField] private List<CellMaterials> _materials;
    [SerializeField] private List<StatusEffectData> statusEffectDatas;
    [SerializeField] private MaterialProvider materialProvider;
    [SerializeField] private UnitNetworkService unitNetworkService;
    [SerializeField] private UnitPrefabManager unitPrefabManager;
    [SerializeField] private SceneLoadWatcher sceneLoadWatcher;

    [SerializeField] private GridRenderStrategy strategy = GridRenderStrategy.PerCell;

    public override void InstallBindings()
    {
        BindStartup();
        BindServices();
        BindConfigurationProviders();
        BindGridRenderer();
        BindModels();
        BindViewModels();
        BindViews();
        BindInputHandlers();
    }

    private void BindInputHandlers()
    {
        Container.Bind<CellInputHandler>().AsSingle();
        // PlayerInputHandler removed - input now handled by GameViewModel events
    }

    private void BindModels()
    {
        Container.Bind<GameModel>().To<GameModel>().AsSingle().NonLazy();
        Container.Bind<IMaterialProvider>().To<MaterialProvider>().FromInstance(materialProvider);

        Dictionary<CellState, CellMaterials> cellMaterials = _materials.ToDictionary(x => x.CellState);
        Container.Bind<IReadOnlyDictionary<CellState, CellMaterials>>().FromInstance(cellMaterials);

        Dictionary<UnitType, UnitDefinitionSO> unitDatas = _unitDatas.ToDictionary(x => x.UnitType);
        Container.Bind<IReadOnlyDictionary<UnitType, UnitDefinitionSO>>().FromInstance(unitDatas);

        // Domain/system-level services
        Container.Bind<UnitModelFactory>().AsSingle();
        Container.Bind<MovementSystem>().AsSingle();
        Container.Bind<TurnSystem>().To<TurnSystem>().AsSingle();
        Container.Bind<SpellZoneFactory>().AsSingle();
        Container.Bind<SpellCasterService>().AsSingle();
        Container.Bind<ActionHandlerFactory>().AsSingle();
    }

    private void BindViewModels()
    {
        Container.Bind<GameViewModel>().To<GameViewModel>().AsSingle().NonLazy();
        Container.Bind<UnitTurnPanelViewModel>().AsSingle().NonLazy();
        Container.Bind<UnitViewModelFactory>().AsSingle();
    }

    private void BindViews()
    {
        Container.Bind<GameView3D>().FromInstance(_gameView3D).AsSingle().NonLazy();
        Container.Bind<IUnitViewResolver>().FromInstance(_gameView3D).AsSingle();
        Container.Bind<UnitTurnPanelView>().FromInstance(_MVVMUnitTurnPanel).AsSingle();
        Container.Bind<GridView>().FromInstance(_gridView).AsSingle().NonLazy();
        Container.Bind<UnitStatsPanel>().FromInstance(_unitStatsPanel).AsSingle();
        Container.Bind<InGameUI>().FromInstance(_inGameUI).AsSingle();
        Container.Bind<IAttackActionPanel>().FromInstance(_attackActionPanel).AsSingle();

        // Presentation services removed - views now subscribe directly to GameViewModel

        // View factories
        Container.Bind<UnitViewFactory>().AsSingle();
    }

    private void BindServices()
    {
        Container.Bind<GameNetworkCommandGateway>().FromInstance(_gameNetworkCommandGateway).AsSingle();
        Container.Bind<SceneLoadWatcher>().FromInstance(sceneLoadWatcher).AsSingle();
        Container.Bind<ClientGameRpcService>().AsSingle();
        Container.Bind<ServerGameRpcService>().AsSingle();
        Container.Bind<NetworkUnitCommandService>().AsSingle();
        Container.Bind<GameController>().FromInstance(_gameController).AsSingle().NonLazy();
        
        // Network services
        Container.Bind<UnitNetworkService>().FromInstance(unitNetworkService).AsSingle();
        Container.Bind<UnitPrefabManager>().FromInstance(unitPrefabManager).AsSingle();

        // Action handler factories
        Container.BindFactory<ICombatObject, MoveActionHandler, MoveActionHandlerFactory>();
        Container.BindFactory<ICombatObject, RangedAttackHandler, RangedAttackHandlerFactory>();
        Container.BindFactory<ICombatObject, MoveThenAttackHandler, MoveThenAttackHandlerFactory>();

        Container.Bind<Transform>()
                 .WithId("UnitsParent")
                 .FromInstance(_gameController.transform);
    }

    private void BindStartup()
    {
        Container.Bind<ISessionRoleProvider>().To<NetcodeSessionRoleProvider>().AsSingle().NonLazy();

        // Выбор стратегии старта по роли/режиму в рантайме
        var roleProvider = Container.Instantiate<NetcodeSessionRoleProvider>();
        Container.Bind<IGameModeProvider>().To<GameModeProvider>().AsSingle();
        switch (roleProvider.CurrentRole)
        {
            case SessionRole.Local:
                Container.Bind<IGameStartupFlow>().To<SinglePlayerGameStartupFlow>().AsSingle();
                Container.Bind<IUnitSpawner>().To<LocalUnitSpawner>().AsSingle();
                Container.Bind<IBattleRunner>().To<LocalBattleRunner>().AsSingle();
                break;
            case SessionRole.Host:
                Container.Bind<IGameStartupFlow>().To<HostGameStartupFlow>().AsSingle();
                Container.Bind<IUnitSpawner>().To<NetworkUnitSpawner>().AsSingle();
                Container.Bind<IBattleRunner>().To<NetworkBattleRunner>().AsSingle();
                break;
            case SessionRole.Client:
                Container.Bind<IGameStartupFlow>().To<ClientGameStartupFlow>().AsSingle();
                Container.Bind<IUnitSpawner>().To<NetworkUnitSpawner>().AsSingle();
                Container.Bind<IBattleRunner>().To<NetworkBattleRunner>().AsSingle();
                break;
        }

    }

    private void BindConfigurationProviders()
    {
        Container.Bind<SceneTransitionDataService>().FromMethod(_ => SceneTransitionDataService.Instance).AsSingle();
        Container.Bind<IBattleEntryProvider>().To<GameSceneConfigurationProvider>().AsSingle();
    }

    private void BindGridRenderer()
    {
        switch (strategy)
        {
            case GridRenderStrategy.Single:
                BindSingle();
                break;
            case GridRenderStrategy.PerCell:
                BindPerCell();
                break;
        }
    }

    private void BindPerCell()
    {
        Container.BindInterfacesTo<PerCellGridRenderer>()
                 .AsSingle()
                 .WithArguments(cellPrefab, _gridView.gameObject, _materials);
    }

    private void BindSingle()
    {
        Container.Bind<IGridCellRenderer>().To<SingleGridRenderer>().AsSingle().WithArguments(cellPrefab, _materials);
    }

    private void UnBind()
    {
        Container.Resolve<IGridCellRenderer>()?.Clear();
    }

    [Button]
    public void SwitchStrategy()
    {
        UnBind();
        BindGridRenderer();
    }
}
