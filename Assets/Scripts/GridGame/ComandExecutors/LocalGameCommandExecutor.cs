public class LocalGameCommandExecutor : IGameCommandExecutor
{
    private readonly ActionPipeline _pipeline;

    public LocalGameCommandExecutor(ActionPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public void StartBattle()
    {
        _pipeline.StartBattle();
    }

    public void Execute(ActionType type, ActionContext ctx)
    {
        _pipeline.Execute(type, ctx);
    }
}
