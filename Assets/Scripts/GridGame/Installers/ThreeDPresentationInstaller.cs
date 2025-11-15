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
        switch (gridRenderStrategy)
        {
            case GridRenderStrategy.Single:
                throw new NotImplementedException();
                //container.BindInterfacesAndSelfTo<SingleGridRenderer>().AsSingle();
            case GridRenderStrategy.PerCell:
                ActivateRenderer(perCellGridRenderer, true);
                ActivateRenderer(tileGridRenderer, false);
                if (perCellGridRenderer != null)
                {
                    container.BindInterfacesAndSelfTo<PerCellGridRenderer>()
                             .FromInstance(perCellGridRenderer)
                             .AsSingle();
                }
                else
                {
                    Debug.LogWarning("[ThreeDPresentationInstaller] PerCellGridRenderer is not assigned.", this);
                }
                break;
            case GridRenderStrategy.Tile:
                ActivateRenderer(perCellGridRenderer, false);
                ActivateRenderer(tileGridRenderer, true);
                if (tileGridRenderer != null)
                {
                    container.BindInterfacesAndSelfTo<TileGridRenderer>()
                             .FromInstance(tileGridRenderer)
                             .AsSingle();
                }
                else
                {
                    Debug.LogWarning("[ThreeDPresentationInstaller] TileGridRenderer is not assigned.", this);
                }
                break;
        }
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





