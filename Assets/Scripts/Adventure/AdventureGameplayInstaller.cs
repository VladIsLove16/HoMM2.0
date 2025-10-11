using Adventure.Application.Dialog;
using Adventure.Domain.Inventory;
using Adventure.Domain.Dialog;
using Adventure.Infrastructure.Interaction;
using Adventure.Infrastructure.Movement;
using Adventure.Infrastructure.Dialog;
using Adventure.Integration.Battle;
using Adventure.Presentation.Mushroom;
using UnityEngine;
using Zenject;
using Adventure.Presentation.Dialog;
using Adventure.Infrastructure.Inventory;

public sealed class AdventureGameplayInstaller : MonoInstaller
{
    [Header("Movement")]
    [SerializeField] private PlayerMovementController playerMovementController;

    [Header("Catalogues")]
    [SerializeField] private UnitDefinitionSOCollection mushroomCatalog;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;

    [Header("Battle")]
    [SerializeField] private GridConfigurationGateway gridConfigurationGateway;
    [SerializeField] private ArmyLineupSO defaultLineup;
    [SerializeField] private int playerFrontlineX = 1;
    [SerializeField] private int enemyFrontlineX = 10;
    [SerializeField] private int rowSpacing = 1;

    [Header("UI & Interaction")]
    [SerializeField] private PlayerInteractionController interactionController;
    [SerializeField] private DialogueUIView dialogueUIView;
    [SerializeField] private MushroomBookView mushroomBookView;

    public override void InstallBindings()
    {
        Container.Bind<UnitDefinitionSOCollection>().FromInstance(mushroomCatalog).AsSingle();
        Container.Bind<IDialogRepository>().FromInstance(dialogueDatabase).AsSingle();
        var resolver = new ArmyFormationResolver(playerFrontlineX, enemyFrontlineX, rowSpacing);
        Container.Bind<ArmyFormationResolver>().FromInstance(resolver).AsSingle(); Container.Bind<MushroomInventoryModel>().AsSingle();

        Container.Bind<IDialogStateStore>().To<PlayerPrefsDialogStateStore>().AsSingle();
        Container.Bind<BattleLaunchService>().AsSingle();
        Container.Bind<BattlePreparationService>().AsSingle();

        Container.Bind<MushroomBookViewModel>().AsSingle();
        Container.Bind<DialogVM>().AsSingle();
        Container.Bind<MushroomCollectionViewModel>().AsSingle();

        Container.Bind<DialogueUIView>().FromInstance(dialogueUIView). AsSingle();
        Container.Bind<MushroomBookView>().FromInstance(mushroomBookView).AsSingle();
        Container.Bind<NpcDialogueTrigger>()
            .FromComponentsInHierarchy()
            .AsTransient();
        Container.Bind<MushroomCollectible>()
            .FromComponentsInHierarchy()
            .AsTransient();
        if (gridConfigurationGateway != null)
        {
            Container.Bind<IGridConfigurationGateway>().FromInstance(gridConfigurationGateway).AsSingle();
        }
        else
        {
            Debug.LogError("GridConfigurationGateway is not assigned on AdventureGameplayInstaller", this);
        }
    }
}
