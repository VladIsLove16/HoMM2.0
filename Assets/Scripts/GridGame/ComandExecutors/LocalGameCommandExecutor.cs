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

    public bool Execute(ActionType type, ActionContext ctx)
    {
        return _pipeline.Execute(type, ctx);
    }
}
