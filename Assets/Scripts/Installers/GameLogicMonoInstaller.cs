using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using static PlayerInputHandler;
public enum GridRenderStrategy { PerCell, Single }

public class GameLogicMonoInstaller : MonoInstaller
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameInputHandler3D _gameInputHandler3D;
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

    [SerializeField] private GridRenderStrategy strategy = GridRenderStrategy.PerCell;

    public override void InstallBindings()
    {
        BindServices();
        BindModels();
        BindViewModels();
        BindGridRenderer();
        BindViews();
        BindInputHandlers();
    }

    private void BindInputHandlers()
    {
        Container.Bind<GameInputHandler3D>().FromInstance(_gameInputHandler3D);
        Container.Bind<PlayerInputHandler>().To<PlayerInputHandler>().AsSingle().NonLazy();
    }

    private void BindModels()
    {
        Container.Bind<GameModel>().To<GameModel>().AsSingle().NonLazy();

        Dictionary<CellState, CellMaterials> cellMaterials = _materials.ToDictionary(x => x.CellState);
        Container.Bind<IReadOnlyDictionary<CellState, CellMaterials>>().FromInstance(cellMaterials);

        //Container.Bind<List>().FromInstance(cellMaterials);

        Dictionary<UnitType, UnitDefinitionSO> unitDatas = _unitDatas.ToDictionary(x => x.UnitType);
        Container.Bind<IReadOnlyDictionary<UnitType, UnitDefinitionSO>>().FromInstance(unitDatas);

        Container.Bind<Transform>()
                 .WithId("UnitsParent")
                 .FromInstance(_gameController.transform);
    }

    private void BindViewModels()
    {
        Container.Bind<GameViewModel>().To<GameViewModel>().AsSingle().NonLazy();
        Container.Bind<UnitTurnPanelViewModel>().AsSingle().NonLazy();
    }

    private void BindViews()
    {
        Container.Bind<GameView3D>().FromInstance(_gameView3D).AsSingle().NonLazy();
        Container.Bind<UnitTurnPanelView>().FromInstance(_MVVMUnitTurnPanel).AsSingle();
        Container.Bind<GridView>().FromInstance(_gridView).AsSingle().NonLazy();
        Container.Bind<UnitStatsPanel>().FromInstance(_unitStatsPanel).AsSingle();
        Container.Bind<GameController>().FromInstance(_gameController).AsSingle().NonLazy();
        Container.Bind<InGameUI>().FromInstance(_inGameUI).AsSingle();
        Container.Bind<IAttackActionPanel>().FromInstance(_attackActionPanel).AsSingle();
        
    }

    private void BindServices()
    {
        Container.Bind<UnitModelFactory>().AsSingle();
        Container.Bind<UnitViewModelFactory>().AsSingle();
        Container.Bind<UnitViewFactory>().AsSingle();
        Container.Bind<TurnSystem>().To<TurnSystem>().AsSingle();
        Container.Bind<MovementSystem>().AsSingle();
        Container.Bind<SpellZoneFactory>().AsSingle();
        Container.Bind<SpellCasterService>().AsSingle();

        Container.BindFactory<ICombatObject, MoveActionHandler, MoveActionHandlerFactory>();
        Container.BindFactory<ICombatObject, RangedAttackHandler, RangedAttackHandlerFactory>();
        Container.BindFactory<ICombatObject, MoveThenAttackHandler, MoveThenAttackHandlerFactory>();

        //Container.Bind<ActionResolver>().To<ActionResolverDebugger>().AsSingle().NonLazy();
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
