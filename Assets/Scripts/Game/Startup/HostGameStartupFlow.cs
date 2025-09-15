using UnityEngine;
using System.Collections;
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
        _controller.Setup(); // создаем сетку
        var mb = _controller as MonoBehaviour;
        if (mb != null)
            mb.StartCoroutine(WaitAndStart());
    }

    private IEnumerator WaitAndStart()
    {
        int safetyFrames = 120;
        while ((_gateway == null || !_gateway.IsSpawned) && safetyFrames-- > 0)
            yield return null;

        if (_gateway == null || !_gateway.IsSpawned)
            throw new System.InvalidOperationException("GameNetworkCommandGateway не готов к RPC");

        // 1️⃣ Создание юнитов на хосте (через IUnitSpawner -> SpawnUnitServerRpc)
        _controller.CreateGridContentFromConfiguration();

        _controller.InitTurnSystem();
        // 2️⃣ Передача клиентам информации о юнитах для TurnSystem
        _gateway.InitClientTurnSystemClientRpc();
        _controller.RunBattle();
        // 3️⃣ Старт боя
        _gateway.StartBattleClientRpc();
    }
}


public class ClientGameStartupFlow : IGameStartupFlow
{
    private readonly GameController _controller;
    public ClientGameStartupFlow(GameController controller)
    {
        _controller = controller;
    }

    public void Run()
    {
        // Создаём сетку локально
        _controller.Setup();
        // Юниты будут созданы через InitClientTurnSystemRpc
        // Бой начнётся через StartBattleRpc
    }
}


public class SinglePlayerGameStartupFlow : IGameStartupFlow
{
    private readonly GameController _controller;
    public SinglePlayerGameStartupFlow(GameController controller) { _controller = controller; }

    public void Run()
    {
        _controller.Setup();
        _controller.CreateGridContentFromConfiguration();
        _controller.InitTurnSystem();
        _controller.RunBattle();
    }
}
