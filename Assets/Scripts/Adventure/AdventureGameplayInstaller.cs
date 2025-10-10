using Adventure.Application.Dialog;
using Adventure.Application.Inventory;
using Adventure.Domain.Inventory;
using Adventure.Domain.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Dialog;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using UnityEngine;
using Zenject;

public sealed class AdventureGameplayInstaller : MonoInstaller
{
    [Header("Movement")]
    [SerializeField] private PlayerMovementController playerMovementController;

    [Header("Catalogues")]
    [SerializeField] private MushroomCatalogSO mushroomCatalog;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;

    [Header("Battle")]
    [SerializeField] private GridConfigurationGateway gridConfigurationGateway;
    [SerializeField] private ArmyLineupSO defaultLineup;
    [SerializeField] private int playerFrontlineX = 1;
    [SerializeField] private int enemyFrontlineX = 10;
    [SerializeField] private int rowSpacing = 1;

    [Header("UI & Interaction")]
    [SerializeField] private PlayerInteractionController interactionController;
    [SerializeField] private AdventureDialogueOrchestrator dialogueOrchestrator;

    public override void InstallBindings()
    {
        Container.Bind<IMushroomCatalog>().FromInstance(mushroomCatalog).AsSingle();
        Container.Bind<MushroomInventoryModel>().AsSingle();
        Container.Bind<Adventure.Application.Inventory.MushroomBookViewModel>().AsSingle();

        Container.Bind<IDialogRepository>().FromInstance(dialogueDatabase).AsSingle();
        Container.Bind<IDialogStateStore>().To<PlayerPrefsDialogStateStore>().AsSingle();
        Container.Bind<DialogVM>().AsSingle();

        var resolver = new ArmyFormationResolver(playerFrontlineX, enemyFrontlineX, rowSpacing);
        Container.Bind<ArmyFormationResolver>().FromInstance(resolver).AsSingle();

        if (gridConfigurationGateway != null)
        {
            Container.Bind<IGridConfigurationGateway>().FromInstance(gridConfigurationGateway).AsSingle();
        }
        else
        {
            Debug.LogError("GridConfigurationGateway is not assigned on AdventureGameplayInstaller", this);
        }

        Container.Bind<BattleLaunchService>().AsSingle();

        Container.QueueForInject(this);
    }

    [Inject]
    private void OnInjected(Adventure.Application.Inventory.MushroomBookViewModel inventoryService, DialogVM dialogService, BattleLaunchService battleLaunchService)
    {
        if (interactionController != null)
        {
            interactionController.Construct(inventoryService);
        }

        if (dialogueOrchestrator != null)
        {
            dialogueOrchestrator.Construct(dialogService, battleLaunchService);
        }
    }
}
