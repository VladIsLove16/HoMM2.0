using Zenject;
public class ConsoleRepresentationInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<ConsoleGridState>().AsSingle();
        Container.Bind<ConsoleGridRenderer>().AsSingle();

        Container.Rebind<IGridCellRenderer>()
                 .FromResolveGetter<ConsoleGridRenderer>(renderer => renderer)
                 .AsSingle();

        Container.Rebind<IWorldToCellProvider>()
                 .FromResolveGetter<ConsoleGridRenderer>(renderer => renderer)
                 .AsSingle();

        Container.BindInterfacesTo<ConsoleGameView>().AsSingle();
    }
}
