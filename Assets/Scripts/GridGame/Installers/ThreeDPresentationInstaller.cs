using System;
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
    [SerializeField] private TileGridRenderer tileGridRenderer;
    [SerializeField] private Transform unitsParent;

    [Header("Grid Rendering")]
    [SerializeField] private GridRenderStrategy gridRenderStrategy = GridRenderStrategy.PerCell;

    public void Install(DiContainer container)
    {
        if (!container.HasBinding<GameViewModel>())
        {
            Debug.LogWarning("[ThreeDPresentationInstaller] GameViewModel binding is missing in this scene. Grid presentation will be skipped.", this);
            DisableBoundObjects();
            return;
        }

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
            container.BindInterfacesAndSelfTo<GridInputRouter>().AsSingle().NonLazy();
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

        var parentTransform = ResolveUnitsParent();
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

    private Transform ResolveUnitsParent()
    {
        if (unitsParent != null)
        {
            return unitsParent;
        }

        if (gameView3D != null)
        {
            return gameView3D.transform;
        }

        return null;
    }

    private void DisableBoundObjects()
    {
        if (gameView3D != null) gameView3D.gameObject.SetActive(false);
        if (cellInputHandler != null) cellInputHandler.gameObject.SetActive(false);
        if (attackActionPanel != null) attackActionPanel.gameObject.SetActive(false);
        if (unitTurnPanel != null) unitTurnPanel.gameObject.SetActive(false);
        if (inGameUI != null) inGameUI.gameObject.SetActive(false);
        if (unitStatsPanel != null) unitStatsPanel.gameObject.SetActive(false);
        if (perCellGridRenderer != null) perCellGridRenderer.gameObject.SetActive(false);
        if (tileGridRenderer != null) tileGridRenderer.gameObject.SetActive(false);
    }

    private void BindGridRenderer(DiContainer container)
    {
        ActivateRenderer(perCellGridRenderer, false);
        ActivateRenderer(tileGridRenderer, false);

        switch (gridRenderStrategy)
        {
            case GridRenderStrategy.PerCell:
                BindRendererInstance(container,
                    perCellGridRenderer,
                    "[ThreeDPresentationInstaller] PerCellGridRenderer");
                break;

            case GridRenderStrategy.Tile:
                BindRendererInstance(container,
                    tileGridRenderer,
                    "[ThreeDPresentationInstaller] TileGridRenderer");
                break;

            case GridRenderStrategy.Single:
                throw new NotImplementedException("Single grid renderer strategy is not implemented yet.");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void BindRendererInstance<T>(DiContainer container, T renderer, string warningContext)
        where T : MonoBehaviour, IGridCellRenderer
    {
        if (renderer == null)
        {
            Debug.LogWarning($"{warningContext} is not assigned.", this);
            return;
        }

        ActivateRenderer(renderer, true);
        container.BindInterfacesAndSelfTo<T>()
                 .FromInstance(renderer)
                 .AsSingle();
    }

    private static void ActivateRenderer(MonoBehaviour renderer, bool active)
    {
        if (renderer == null)
            return;

        if (renderer.gameObject.activeSelf != active)
        {
            renderer.gameObject.SetActive(active);
        }
        renderer.enabled = active;
    }
}





