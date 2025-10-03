using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Zenject;
public enum GridRenderStrategy { PerCell, Single }

public class GameLogicMonoInstaller : MonoInstaller
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameNetworkCommandGateway _gameNetworkCommandGateway;
    [SerializeField] private GameView3D _gameView3D;
    [SerializeField] private CellInputHandler _cellInputHandler;
    [SerializeField] private AttackActionPanel _attackActionPanel;
    [SerializeField] private UnitTurnPanelView _MVVMUnitTurnPanel;
    [SerializeField] private InGameUI _inGameUI;
    [SerializeField] private UnitStatsPanel _unitStatsPanel;
    [SerializeField] private GridView _gridView;
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private GameObject singleGridPrefab;
    [SerializeField] private GameUnitDatas _unitDatas;
    [SerializeField] private List<StatusEffectData> statusEffectDatas;
    [SerializeField] private MaterialProvider materialProvider;
    [SerializeField] private UnitPrefabManager unitPrefabManager;
    [SerializeField] private SceneLoadWatcher sceneLoadWatcher;
    [SerializeField] private PerCellGridRenderer perCellGridRenderer;
    [SerializeField] private CursorService CursorService;

    [SerializeField] private GridRenderStrategy strategy = GridRenderStrategy.PerCell;

    public override void InstallBindings()
    {
        BindServices();
        BindConfigurationProviders();
        BindModels();
        BindViewModels();
        BindGridRenderer();
        BindViews();
        BindInputHandlers();
    }

    private void BindInputHandlers()
    {
        Container.Bind<CellInputHandler>().FromInstance(_cellInputHandler).  AsSingle();
    }

    private void BindModels()
    {
        IReadOnlyDictionary<UnitType, UnitDefinitionSO> unitDatas = _unitDatas.ToDictionary();
        Container.Bind<IReadOnlyDictionary<UnitType, UnitDefinitionSO>>().FromInstance(unitDatas);

        Container.Bind<UnitModelFactory>().AsSingle().NonLazy();
        Container.Bind<MovementSystem>().AsSingle();
        Container.Bind<ActionResolver>().AsSingle();
        Container.Bind<TurnSystem>().AsSingle();
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

    private void BindViews()
    {
        Container.Bind<IMaterialProvider>().FromInstance(materialProvider);

        Container.Bind<GameView3D>().FromInstance(_gameView3D).AsSingle().NonLazy();
        Container.Bind<UnitTurnPanelView>().FromInstance(_MVVMUnitTurnPanel).AsSingle();
        Container.Bind<GridView>().FromInstance(_gridView).AsSingle().NonLazy();
        Container.Bind<UnitStatsPanel>().FromInstance(_unitStatsPanel).AsSingle();
        Container.Bind<InGameUI>().FromInstance(_inGameUI).AsSingle();
        Container.Bind<IAttackActionPanel>().FromInstance(_attackActionPanel).AsSingle();

        Container.Bind<UnitViewFactory>().AsSingle();
        Container.Bind<GameController>().FromInstance(_gameController).AsSingle().NonLazy();
        Container.Bind<Transform>()
                 .WithId("UnitsParent")
                 .FromInstance(_gameController.transform);

    }

    private void BindServices()
    {
        Container.Bind<GameNetworkCommandGateway>().FromInstance(_gameNetworkCommandGateway).AsSingle();
        Container.Bind<SceneLoadWatcher>().FromInstance(sceneLoadWatcher).AsSingle();
        Container.Bind<UnitPrefabManager>().FromInstance(unitPrefabManager).AsSingle();
        Container.Bind<ICursorService>().FromInstance(CursorService).AsSingle().NonLazy();
    }

    private void BindConfigurationProviders()
    {
        Container.Bind<SceneTransitionDataService>().FromMethod(_ => SceneTransitionDataService.Instance).AsSingle();
        Container.Bind<IGameModeProvider>().FromResolve().AsSingle();
        Container.Bind<GameSceneConfigurationProvider>().AsSingle();

        var mode = SceneTransitionDataService.Instance != null ? SceneTransitionDataService.Instance.CurrentGameMode : GameMode.SinglePlayer;
        if (mode == GameMode.SinglePlayer)
            Container.Bind<IGameCommandExecutor>().To<LocalGameCommandExecutor>().AsSingle();
        else
            Container.Bind<IGameCommandExecutor>().To<NetworkGameCommandExecutor>().AsSingle();
        Container.Bind<ActionPipeline>().AsSingle();

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
        var renderer = Container.Resolve<IGridCellRenderer>();
        var vm = Container.Resolve<IGridViewModel>();
        renderer.Bind(vm);
    }

    private void BindPerCell()
    {
        Container.BindInterfacesTo<PerCellGridRenderer>()
                .FromInstance(perCellGridRenderer)
                .AsSingle();
    }

    private void BindSingle()
    {
        Container.Bind<IGridCellRenderer>().To<SingleGridRenderer>().AsSingle();
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
