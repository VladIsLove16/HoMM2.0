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
        _controller.InitCombatSystem();
    }
}


