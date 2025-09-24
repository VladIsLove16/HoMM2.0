public class AttackActionHandler : IActionHandler
{
    MovementSystem _movementSystem;
    GameModel _gameModel;
    public AttackActionHandler(MovementSystem movementSystem, GameModel gameModel)
    {
        _movementSystem = movementSystem;
        _gameModel = gameModel;
    }
    public void Execute(ActionContext ctx)
    {
        var attacker = _gameModel.GetCell(ctx.FromCell).Unit as IDamageSource;
        var target = _gameModel.GetCell(ctx.TargetCell).Unit as IDamagable;
        attacker.SendDamage(new(target, true));
    }
    public bool CanExecute(ActionContext ctx)
    {
        return true;
    }
}