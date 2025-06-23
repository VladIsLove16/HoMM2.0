using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameLogicMonoInstaller : MonoInstaller
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameView3D _gameView3D;
    [SerializeField] private GridView _gridView;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject singleGridPrefab;
    [SerializeField] private UnitDefinitionSO[] _unitDatas;
    [SerializeField] private Material cellSelectMaterial;

    public enum GridRenderStrategy { PerCell, Single }
    [SerializeField] private GridRenderStrategy strategy = GridRenderStrategy.PerCell;

    private ICellGridRenderer _currentRenderer;
    public override void InstallBindings()
    {
        Container.Bind<UnitViewFactory>().AsSingle();
        Container.Bind<UnitModelFactory>().AsSingle();

        Container.Bind<GameModel>().AsSingle();

        Container.Bind<GameViewModel>()
         .AsSingle()
         .NonLazy();

        Container.Bind<GameView3D>()
         .FromInstance(_gameView3D)
         .AsSingle();
         

        Container.Bind<GridView>()
        .FromInstance(_gridView)
        .AsSingle()
        .NonLazy();

        Container.Bind<GameController>().
        FromInstance(_gameController)
        .AsSingle()
        .NonLazy();

        BindPerCell();
        Container.Bind<IEnumerable<UnitDefinitionSO>>()
                 .FromInstance(_unitDatas);

        Container.Bind<CombatController>().AsSingle();


        Container.Bind<Transform>()
                 .WithId("UnitsParent")
                 .FromInstance(_gameController.transform);

        Container.Bind<SpellZoneFactory>().AsSingle();
        Container.Bind<SpellCasterService>().AsSingle();

        Container.Bind<InputManager>().AsSingle().NonLazy();
    }
    private void BindPerCell()
    {
        Container.Bind<ICellGridRenderer>().To<PerCellGridRenderer>().AsSingle().WithArguments(cellPrefab, _gridView.gameObject, cellSelectMaterial);
        _currentRenderer = Container.Resolve<ICellGridRenderer>();
    }
    private void BindSingle()
    {
        Container.Bind<ICellGridRenderer>().To<SingleGridRenderer>().AsSingle().WithArguments(cellPrefab, cellSelectMaterial);
        _currentRenderer = Container.Resolve<ICellGridRenderer>();
    }
    private void UnBind()
    {
        Container.Unbind<ICellGridRenderer>();
        _currentRenderer?.Clear();
    }
    [Button]
    public void SwitchStrategy()
    {
        UnBind();
        switch (strategy)
        {
            case GridRenderStrategy.Single:
                {
                    BindPerCell();
                    break;
                }
            case GridRenderStrategy.PerCell:
                {
                    BindSingle();
                    break;
                }
        }
    }
    //private void SwitchRendererToSingle()
    //{
    //    var newRenderer = new SingleGridRenderer(cellPrefab, cellSelectMaterial);

    //    // Обновим зависимость в контейнере
    //    Container.Unbind<ICellGridRenderer>();
    //    BindSingle();

    //    // Обновим визуал
    //    _currentRenderer?.Clear();
    //    _currentRenderer = newRenderer;

    //    var _gameViewModel = Container.Resolve<GameViewModel>();
    //    _gridView.Construct(_gameViewModel, _currentRenderer);
    //    _gridView.CreateGrid();
    //}
    //private void SwitchRendererToPerCell()
    //{
    //    var newRenderer = new PerCellGridRenderer(cellPrefab, _gridView.gameObject, cellSelectMaterial);

    //    BindPerCell();

    //    _currentRenderer?.Clear();
    //    _currentRenderer = newRenderer;

    //    var _gameViewModel = Container.Resolve<GameViewModel>();
    //    _gridView.Construct(_gameViewModel, _currentRenderer);
    //    _gridView.CreateGrid();
    //}
}
