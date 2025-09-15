using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ServerGameRpcService
{
    private readonly GameModel _gameModel;
    private System.Action<UnitModel> _markDirty;
    private readonly HashSet<UnitModel> _subscribedUnits = new HashSet<UnitModel>();

    [Inject]
    public ServerGameRpcService(GameModel gameModel)
    {
        _gameModel = gameModel;
    }

    public void SetMarkDirtyCallback(System.Action<UnitModel> markDirty)
    {
        _markDirty = markDirty;
    }

    public void ApplyServerMove(Vector2Int startCell, List<Vector2Int> routeList)
    {
        var unit = _gameModel.GetCell(startCell).Unit;
        if (unit == null) return;
        EnsureSubscribed(unit);
        _gameModel.MoveObject(unit, routeList);
        _markDirty?.Invoke(unit);
    }

    public void ApplyServerAttack(Vector2Int attackerCell, Vector2Int targetCell)
    {
        var attacker = _gameModel.GetCell(attackerCell).Unit;
        var target = _gameModel.GetCell(targetCell).Unit as IDamagable;
        if (attacker == null || target == null) return;
        EnsureSubscribed(attacker);
        if (target is UnitModel targetUnit) EnsureSubscribed(targetUnit);
        attacker.SendDamage(new AttackContext(target));
        _markDirty?.Invoke(attacker);
        if (target is UnitModel tm) _markDirty?.Invoke(tm);
    }

    public void ApplyServerSpawn(int x, int y, UnitType unitType, int amount, bool isPlayer)
    {
        Debug.Log("[ServerGameRpcService] ApplyServerSpawn "  + unitType);
        _gameModel.SpawnUnit(new UnitSpawnParams(x, y, unitType, amount, isPlayer));
        var unit = _gameModel.GetCell(new Vector2Int(x, y)).Unit;
        if (unit != null)
        {
            EnsureSubscribed(unit);
            _markDirty?.Invoke(unit);
        }
    }

    public void ApplyServerRemove(Vector2Int cell)
    {
        var unit = _gameModel.GetCell(cell).Unit;
        if (unit == null) return;
        unit.Died?.Invoke();
    }

    private void EnsureSubscribed(UnitModel unit)
    {
        if (unit == null) return;
        if (_subscribedUnits.Contains(unit)) return;
        _subscribedUnits.Add(unit);

        unit.HealthChanged += () => _markDirty?.Invoke(unit);
        unit.StatsChanged += () => _markDirty?.Invoke(unit);
        //unit.Amount.Subscribe(_ => _markDirty?.Invoke(unit));
        unit.Died += () => _subscribedUnits.Remove(unit);
    }
}



