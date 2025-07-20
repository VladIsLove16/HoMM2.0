public class TurnHandler
{
    private readonly ActionService _actionService;
    private readonly MovementSystem _movementSystem;

    public TurnHandler(ActionService actionService, MovementSystem movementSystem)
    {
        _actionService = actionService;
        _movementSystem = movementSystem;
    }

    public void HandleTurnStart(UnitModel unit, GameCell currentCell)
    {
        var moveAction = new MoveUnitAcion(
            _movementSystem,
            currentCell,
            unit.ModifiedStats.MoveSpeed,
            (route) => {
                // можно подключить отрисовку маршрута или событие
            });

        _actionService.SetAction(moveAction);
    }
}
