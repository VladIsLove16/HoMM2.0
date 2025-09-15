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
        _controller.InitCombatSystem();
        _controller.CreateGridContentFromConfiguration();
        _controller.RunBattle();
    }
}


