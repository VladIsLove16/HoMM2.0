using UnityEngine;

public interface IGameCommandExecutor
{
    void StartBattle();
    bool Execute(ActionType type, ActionContext ctx);
    bool TryDeployUnit(Vector2Int fromCell, Vector2Int toCell);
}
