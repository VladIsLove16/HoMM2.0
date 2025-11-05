using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public sealed class GridCommander
{
    private static readonly HashSet<UnitType> FungusTypes = new()
    {
        UnitType.BaseMushroom,
        UnitType.BlueMushroom,
        UnitType.DoubleMushroom,
        UnitType.HumanizedMushroom
    };

    private readonly GameModel _gameModel;

    public GridCommander(DiContainer container)
    {
        if (container == null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        _gameModel = container.TryResolve<GameModel>();
    }

    public void SpawnFungus(int x, int y, UnitType unitType, IDeveloperConsoleOutput output)
    {
        if (!EnsureContextAvailable(output))
        {
            return;
        }

        if (!IsFungus(unitType))
        {
            output.AppendError($"Тип '{unitType}' не является грибом. Используйте один из: {string.Join(", ", FungusTypes)}.");
            return;
        }

        var result = _gameModel.SpawnUnit(new UnitSpawnParams(x, y, unitType, 1, Team.Red));
        if (!result.IsSuccess)
        {
            var message = string.IsNullOrEmpty(result.Message) ? "Не удалось создать гриб." : result.Message;
            output.AppendError(message);
            return;
        }

        output.AppendLine($"Гриб '{unitType}' создан в клетке ({x},{y}).");
    }

    public void KillFungus(int x, int y, IDeveloperConsoleOutput output)
    {
        if (!EnsureContextAvailable(output))
        {
            return;
        }

        if (!_gameModel.IsInBounds(new Vector2Int(x, y)))
        {
            output.AppendError("Координаты вне поля.");
            return;
        }

        if (_gameModel.GetCell(new Vector2Int(x, y)) is not GameCell cell)
        {
            output.AppendError("Не удалось получить клетку.");
            return;
        }

        var unit = cell.Unit;
        if (unit == null)
        {
            output.AppendError("В указанной клетке нет юнита.");
            return;
        }

        if (!IsFungus(unit.UnitType.Value))
        {
            output.AppendError($"Юнит в клетке ({x},{y}) не является грибом (обнаружен '{unit.UnitType.Value}').");
            return;
        }

        var damageContext = new DamageContext(int.MaxValue, DamageType.physical, unit);
        unit.RecieveDamage(damageContext);
        cell.TryRemoveUnit(unit);

        output.AppendLine($"Гриб в клетке ({x},{y}) удалён.");
    }

    private bool EnsureContextAvailable(IDeveloperConsoleOutput output)
    {
        if (_gameModel != null)
        {
            return true;
        }

        output?.AppendError("Сеточный командер недоступен в этой сцене.");
        return false;
    }

    private static bool IsFungus(UnitType unitType) => FungusTypes.Contains(unitType);
}
