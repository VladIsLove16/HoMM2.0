using UnityEngine;
using Zenject;

/// <summary>
/// Installs the developer console service, runtime view, and all built-in commands.
/// Attach this installer to a scene context to enable the console.
/// </summary>
public class DeveloperConsoleInstaller : MonoInstaller
{
    [SerializeField] private DeveloperConsoleView consoleView;

    public override void InstallBindings()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!HasRequiredBindings())
        {
            Debug.LogWarning("[DeveloperConsoleInstaller] Skipping console setup - required gameplay services are missing in this scene.");
            return;
        }

        BindCommands();
        Container.Bind<AdventureCommander>().AsSingle();
        Container.Bind<GridCommander>().AsSingle();
        Container.BindInterfacesAndSelfTo<DeveloperConsoleService>().AsSingle();

        if (consoleView != null)
        {
            Container.Bind<DeveloperConsoleView>().FromInstance(consoleView).AsSingle();
            Container.QueueForInject(consoleView);
        }

        Container.BindInterfacesTo<DeveloperConsoleInput>().AsSingle();
#else
        Debug.Log("[DeveloperConsoleInstaller] Build variant doesn't include the developer console.");
#endif
    }

    private void BindCommands()
    {
        Container.Bind<IDeveloperConsoleCommand>().To<SpawnUnitConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<HoverCellConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<SelectCellConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<ExecuteActionConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<ListUnitsConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<StartBattleConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<EndTurnConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<InspectCellConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<AdventureCreateNpcConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<AdventureAddNpcArmyConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<AdventureAddPlayerArmyConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<AdventureSpawnFungusConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<GridSpawnFungusConsoleCommand>().AsTransient();
        Container.Bind<IDeveloperConsoleCommand>().To<GridKillFungusConsoleCommand>().AsTransient();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private bool HasRequiredBindings()
    {
        return Container.HasBinding<GameModel>()
            && Container.HasBinding<GameViewModel>()
            && Container.HasBinding<IGameCommandExecutor>()
            && Container.HasBinding<ITurnService>()
            && Container.HasBinding<ActionResolver>();
    }
#endif
}
