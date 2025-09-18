using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Доменный интерфейс для выполнения команд юнита
/// </summary>
public interface IUnitCommandExecutor
{
    bool CanExecuteCommands { get; }
    void ExecuteMoveCommand(List<Vector2Int> route);
    void ExecuteAttackCommand(ulong targetUnitId);
    System.Action<List<Vector2Int>> OnMoveCommandReceived { get; set; }
    System.Action<ulong> OnAttackCommandReceived { get; set; }
}

/// <summary>
/// Доменный интерфейс для сетевого контроллера юнита
/// </summary>
public interface IUnitNetworkController
{
    bool CanExecuteCommands { get; }
    void RequestMove(List<Vector2Int> route);
    void RequestAttack(ulong targetUnitId);
    System.Action<List<Vector2Int>> OnMoveCommandReceived { get; set; }
    System.Action<ulong> OnAttackCommandReceived { get; set; }
}
