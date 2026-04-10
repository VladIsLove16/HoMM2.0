using UnityEngine;

public class LocalGameCommandExecutor : IGameCommandExecutor
{
    private readonly ActionPipeline _pipeline;
    private readonly GameModel _gameModel;

    public LocalGameCommandExecutor(ActionPipeline pipeline, GameModel gameModel)
    {
        _pipeline = pipeline;
        _gameModel = gameModel;
    }

    public void StartBattle()
    {
        _pipeline.StartBattle();
    }

    public bool Execute(ActionType type, ActionContext ctx)
    {
        return _pipeline.Execute(type, ctx);
    }

    public bool TryDeployUnit(Vector2Int fromCell, Vector2Int toCell)
    {
        var unit = _gameModel?.GetCell(fromCell)?.Unit;
        if (unit == null)
            return false;

        _gameModel.MoveObject(unit, toCell);
        return true;
    }
}
