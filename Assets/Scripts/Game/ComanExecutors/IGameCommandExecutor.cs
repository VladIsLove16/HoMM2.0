public interface IGameCommandExecutor
{
    void StartBattle();
    void Execute(ActionType type, ActionContext ctx);
}
