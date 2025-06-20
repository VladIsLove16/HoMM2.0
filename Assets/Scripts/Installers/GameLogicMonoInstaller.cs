using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameLogicMonoInstaller : MonoInstaller
{
    [SerializeField] private GridController gameController;
    private GameGridModel gridModel;
    private GameGridViewModel gameGridViewModel;
    [SerializeField] private GridView gridView;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject singleGridPrefab;
    [SerializeField] private UnitDefinitionSO[] unitDatas;

    public enum GridRenderStrategy { PerCell, Single }
    [SerializeField] private GridRenderStrategy strategy = GridRenderStrategy.PerCell;

    private ICellGridRenderer _currentRenderer;
    public override void InstallBindings()
    {
        switch (strategy)
        {
            case GridRenderStrategy.PerCell:
                BindPerCell();
                break;
            case GridRenderStrategy.Single:
                BindSingle();
                break;
        }
        //Container.Bind<IGridContentFactory>().To<UnitGridContentFactory>().AsSingle();

        Container.Bind<UnitViewFactory>().AsSingle();
        Container.Bind<UnitModelFactory>().AsSingle();

        Container.Bind<GameGridModel>().AsSingle();
        Container.Bind<GameGridViewModel>()
         .AsSingle()
         .NonLazy();

        Container.Bind<GameGridView3D>()
         .AsSingle()
         .NonLazy();

        Container.Bind<GridView>()
        .FromInstance(gridView)
        .AsSingle()
        .NonLazy();

        Container.Bind<GridController>()
        .FromComponentInHierarchy()
        .AsSingle();

        Container.Bind<IEnumerable<UnitDefinitionSO>>()
                 .FromInstance(unitDatas);

        Container.Bind<CombatController>().AsSingle();


        Container.Bind<Transform>()
                 .WithId("UnitsParent")
                 .FromInstance(gameController.transform);

        Container.Bind<SpellZoneFactory>().AsSingle();
        Container.Bind<SpellCasterService>().AsSingle();
    }
    private void BindPerCell()
    {
        Container.Bind<ICellGridRenderer>().To<PerCellGridRenderer>().AsSingle().WithArguments(cellPrefab, gridView.gameObject);
    }
    private void BindSingle()
    {
        Container.Bind<ICellGridRenderer>().To<SingleGridRenderer>().AsSingle().WithArguments(cellPrefab);

    }
    [Button]
    public void SwitchStrategy()
    {
        switch (strategy)
        {
            case GridRenderStrategy.Single:
                {
                    SwitchRendererToPerCell();
                    break;
                }
            case GridRenderStrategy.PerCell:
                {
                    SwitchRendererToSingle();
                    break;
                }
        }
    }
    private void SwitchRendererToSingle()
    {
        var newRenderer = new SingleGridRenderer(cellPrefab);

        // Обновим зависимость в контейнере
        Container.Unbind<ICellGridRenderer>();
        BindSingle();

        // Обновим визуал
        _currentRenderer?.Clear();
        _currentRenderer = newRenderer;

        var viewModel = Container.Resolve<GameGridViewModel>();
        gridView.Construct(viewModel, _currentRenderer);
        gridView.CreateGrid();
    }
    private void SwitchRendererToPerCell()
    {
        var newRenderer = new PerCellGridRenderer(cellPrefab, gridView.gameObject);

        Container.Unbind<ICellGridRenderer>();
        BindPerCell();

        _currentRenderer?.Clear();
        _currentRenderer = newRenderer;

        var viewModel = Container.Resolve<GameGridViewModel>();
        gridView.Construct(viewModel, _currentRenderer);
        gridView.CreateGrid();
    }
}
