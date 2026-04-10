using UnityEngine;
using Zenject;

public sealed class LobbyRoot : MonoInstaller
{
    [Header("Scene References")]
    [SerializeField] private LobbyManager lobbyManager;

    public override void InstallBindings()
    {
        if (lobbyManager == null)
            lobbyManager = GetComponent<LobbyManager>();

        if (lobbyManager == null)
        {
            Debug.LogError("[LobbyRoot] LobbyManager is not assigned.", this);
            return;
        }

        Container.Bind<LobbyManager>().FromInstance(lobbyManager).AsSingle();
    }
}
