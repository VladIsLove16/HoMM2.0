using UnityEngine;
using Zenject;

public class ThreeDPresentationInstaller : MonoBehaviour, IGamePresentationInstaller
{
    [Header("3D Presentation References")]
    [SerializeField] private GameView3D gameView3D;
    [SerializeField] private CellInputHandler cellInputHandler;
    [SerializeField] private AttackActionPanel attackActionPanel;
    [SerializeField] private UnitTurnPanelView unitTurnPanel;
    [SerializeField] private InGameUI inGameUI;
    [SerializeField] private UnitStatsPanel unitStatsPanel;
    [SerializeField] private MaterialProvider materialProvider;
    [SerializeField] private PerCellGridRenderer perCellGridRenderer;
    [SerializeField] private Transform unitsParent;

    [Header("Grid Rendering")]
    [SerializeField] private GridRenderStrategy gridRenderStrategy = GridRenderStrategy.PerCell;

    public void Install(DiContainer container)
    {
        if (gameView3D != null)
        {
            gameView3D.gameObject.SetActive(true);
            container.Bind<GameView3D>().FromInstance(gameView3D).AsSingle().NonLazy();
        }

        if (cellInputHandler != null)
        {
            cellInputHandler.gameObject.SetActive(true);
            cellInputHandler.enabled = true;
            container.Bind<CellInputHandler>().FromInstance(cellInputHandler).AsSingle();
        }

        BindViews(container);
        BindGridRenderer(container);
    }

    private void BindViews(DiContainer container)
    {
        if (materialProvider != null)
        {
            container.Bind<IMaterialProvider>().FromInstance(materialProvider).AsSingle();
        }

        var parentTransform = ResolveUnitsParent(container);
        if (parentTransform != null)
        {
            container.Bind<Transform>()
                     .WithId("UnitsParent")
                     .FromInstance(parentTransform)
                     .AsSingle();
        }

        if (unitTurnPanel != null)
        {
            container.Bind<UnitTurnPanelView>().FromInstance(unitTurnPanel).AsSingle();
        }

        if (unitStatsPanel != null)
        {
            container.Bind<UnitStatsPanel>().FromInstance(unitStatsPanel).AsSingle();
        }

        if (inGameUI != null)
        {
            container.Bind<InGameUI>().FromInstance(inGameUI).AsSingle();
        }

        if (attackActionPanel != null)
        {
            container.Bind<IAttackActionPanel>().FromInstance(attackActionPanel).AsSingle();
        }

        container.Bind<UnitViewFactory>().AsSingle();
    }

    private Transform ResolveUnitsParent(DiContainer container)
    {
        if (unitsParent != null)
        {
            return unitsParent;
        }

        if (gameView3D != null)
        {
            return gameView3D.transform;
        }

        if (container.HasBinding<GameController>())
        {
            return container.Resolve<GameController>().transform;
        }

        return null;
    }

    private void BindGridRenderer(DiContainer container)
    {
        switch (gridRenderStrategy)
        {
            case GridRenderStrategy.Single:
                container.Bind<IGridCellRenderer>().To<SingleGridRenderer>().AsSingle();
                break;
            case GridRenderStrategy.PerCell:
                container.BindInterfacesTo<PerCellGridRenderer>()
                         .FromInstance(perCellGridRenderer)
                         .AsSingle();
                break;
        }

        var renderer = container.Resolve<IGridCellRenderer>();
        var viewModel = container.Resolve<IGridViewModel>();
        renderer.Bind(viewModel);
    }
}


