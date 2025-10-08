using UnityEngine;
using Zenject;

public class ConsolePresentationInstaller : MonoBehaviour, IGamePresentationInstaller
{
    [SerializeField] private Behaviour[] behavioursToDisable;

    public void Install(DiContainer container)
    {
        if (behavioursToDisable != null)
        {
            foreach (var behaviour in behavioursToDisable)
            {
                if (behaviour == null) continue;
                behaviour.enabled = false;
            }
        }

        container.Bind<ConsoleGridState>().AsSingle();
        container.Bind<ConsoleGridRenderer>().AsSingle();
        container.Rebind<IGridCellRenderer>()
                 .FromResolveGetter<ConsoleGridRenderer>(renderer => renderer)
                 .AsSingle();
        container.Rebind<IWorldToCellProvider>()
                 .FromResolveGetter<ConsoleGridRenderer>(renderer => renderer)
                 .AsSingle();
        container.BindInterfacesTo<ConsoleGameView>().AsSingle();
    }
}
