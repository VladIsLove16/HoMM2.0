public interface IGameCommandExecutor
{
    void StartBattle();
    bool Execute(ActionType type, ActionContext ctx);
}
