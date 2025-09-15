using UnityEngine;
using Zenject;

public class ClientGameRpcService
{
    private readonly GameModel _gameModel;

    [Inject]
    public ClientGameRpcService(GameModel gameModel)
    {
        _gameModel = gameModel;
    }

    public void ApplyClientAttack(Vector2Int attackerCell, Vector2Int targetCell)
    {
        var attacker = _gameModel.GetCell(attackerCell).Unit;
        var target = _gameModel.GetCell(targetCell).Unit as IDamagable;
        if (attacker == null || target == null) return;
        attacker.SendDamage(new AttackContext(target));
    }

    public void ApplyUnitState(GameNetworkCommandGateway.UnitState s)
    {
        var unit = _gameModel.GetCell(new Vector2Int(s.CellX, s.CellY)).Unit;
        if (unit == null) return;
        bool statsChanged = false;
        bool amountChanged = unit.Amount.Value != s.Amount;
        unit.Amount.SetValueAndForceNotify(s.Amount);
        unit.IsBlueTeam.SetValueAndForceNotify(s.IsBlueTeam);
        if (unit.ModifiedStats != null)
        {
            if (unit.ModifiedStats.Health != s.Health) { unit.ModifiedStats.Health = s.Health; statsChanged = true; }
            if (unit.ModifiedStats.MaxHealth != s.MaxHealth) { unit.ModifiedStats.MaxHealth = s.MaxHealth; statsChanged = true; }
        }
        if (statsChanged) unit.HealthChanged?.Invoke();
        if (statsChanged) unit.StatsChanged?.Invoke();
    }
}




