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
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.enabled = false;
            }
        }

        container.Bind<ConsoleGridState>().AsSingle();
        container.Bind<ConsoleGridRenderer>().AsSingle();
        container.Bind<IGridCellRenderer>().To<ConsoleGridRenderer>().FromResolve().AsSingle();
        container.Bind<IWorldToCellProvider>().To<ConsoleGridRenderer>().FromResolve().AsSingle();
        container.BindInterfacesTo<ConsoleGameView>().AsSingle();
    }
}
