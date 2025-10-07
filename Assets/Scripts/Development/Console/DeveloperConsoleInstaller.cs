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
        BindCommands();
        Container.BindInterfacesAndSelfTo<DeveloperConsoleService>().AsSingle();

        if (consoleView != null)
        {
            Container.QueueForInject(consoleView);
        }
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
    }
}
