using UnityEngine;

public class HostGameStartupFlow : IGameStartupFlow
{
    private readonly GameController _controller;
    private readonly GameNetworkCommandGateway _gateway;

    public HostGameStartupFlow(GameController controller, GameNetworkCommandGateway gateway)
    {
        _controller = controller;
        _gateway = gateway;
    }

    public void Run()
    {
        _controller.Setup(8, 8);
        var mb = _controller as MonoBehaviour;
        if (mb != null)
        {
            mb.StartCoroutine(WaitAndStart());
        }
    }

    private System.Collections.IEnumerator WaitAndStart()
    {
        _controller.CreateGridContentFromConfiguration();
        int safetyFrames = 120;
        while ((_gateway == null || !_gateway.IsSpawned) && safetyFrames-- > 0)
        {
            yield return null;
        }
        if (_gateway == null || !_gateway.IsSpawned)
        {
            throw new System.InvalidOperationException("GameNetworkCommandGateway is not spawned in time");
        }
        _controller.InitTurnSystemClientRpc();
        _controller.RunBattleClientRpc();
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
        _controller.Setup();
    }
}


public class SinglePlayerGameStartupFlow : IGameStartupFlow
{
    private readonly GameController _controller;
    public SinglePlayerGameStartupFlow(GameController controller)
    {
        _controller = controller;
    }
    public void Run()
    {
        _controller.Setup(8, 8);
        _controller.CreateGridContentFromConfiguration();
        _controller.InitTurnSystemClientRpc();
        _controller.RunBattleClientRpc();
    }
}


