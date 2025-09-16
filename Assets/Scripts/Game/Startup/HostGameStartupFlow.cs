using System.Collections;
using UnityEngine;
using Zenject;

public class HostGameStartupFlow : IGameStartupFlow
{
    private readonly GameController _controller;
    private readonly GameModel _gameModel;
    private readonly GameNetworkCommandGateway _gateway;
    [Inject]
    public HostGameStartupFlow(GameController controller, GameNetworkCommandGateway gateway, GameModel gameModel)
    {
        _controller = controller;
        _gateway = gateway;
        _gameModel = gameModel;
    }

    public void Run()
    {
        //отправляем клиентам данные о сетке
        _gateway.TrySendBattleSetup();

        _controller.Setup(); // создаем сетку
        _controller.CreateGridContentFromConfiguration();
        // Инициализируем локальный TurnSystem
        _controller.InitTurnSystem();
        // Старт боя локально на хосте
        _controller.RunBattle();
    }
}


public class ClientGameStartupFlow : IGameStartupFlow
{
    private readonly GameController _controller;
    private readonly GameNetworkCommandGateway _gateway;
    private SceneLoadWatcher _sceneLoadWatcher;

    public ClientGameStartupFlow(GameController controller, GameNetworkCommandGateway gateway, SceneLoadWatcher sceneLoadWatcher)
    {
        _controller = controller;
        _gateway = gateway;
        _sceneLoadWatcher = sceneLoadWatcher;
    }

    public void Run()
    {
        _sceneLoadWatcher.OnSceneReady += () => {
            Debug.Log("client scene ready!") ;
            _gateway.ClientSceneLoadedServerRpc();
        };
    }
}


public class SinglePlayerGameStartupFlow : IGameStartupFlow
{
    private readonly GameController _controller;
    public SinglePlayerGameStartupFlow(GameController controller) { _controller = controller; }

    public void Run()
    {
        Debug.Log("running singleplayerFlow");
        _controller.Setup();
        _controller.CreateGridContentFromConfiguration();
        _controller.InitTurnSystem();
        _controller.RunBattle();
    }
}
