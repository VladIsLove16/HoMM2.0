using System.Collections.Generic;
using Vector2Int = UnityEngine.Vector2Int;
using Debug = UnityEngine.Debug;

public class GameModelDebugger : GameModel
{
    public GameModelDebugger(UnitModelFactory factory, MovementSystem movement) 
        : base(factory, movement )
    {
        Debug.Log("GameModel is ready");
    }

    protected override GameCell CreateEmptyGameGridObject(GridXZ<GameCell> grid, int x, int y)
    {
        //Debug.Log("Creating endCell in " + x + ":" + y);
        return base.CreateEmptyGameGridObject(grid, x, y);
    }
    public override void InitializeGrid(int width, int height)
    {
        Debug.Log("Game model InitializeGrid called " + width + " " + height);
        base.InitializeGrid(width, height);
    }
    public override OperationResult SpawnUnit(UnitSpawnParams spawnParams)
    {
        Debug.Log("model.SpawnUnit called" + spawnParams.UnitType);
        return base.SpawnUnit(spawnParams);
    }
    public override void MoveObject(IMoveable unit, List<Vector2Int> path)
    {
         Debug.Log("MoveObject callled " + unit + " " + path.ToString());
         base.MoveObject(unit, path);
    }
}
