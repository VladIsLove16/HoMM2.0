using System.Collections.Generic;
using UnityEngine;
using Game.Network;
using System;

/// <summary>
/// Локальный исполнитель команд для одиночной игры
/// </summary>
public class LocalUnitCommandExecutor : MonoBehaviour, IUnitCommandExecutor
{
    public bool CanExecuteCommands => true; // В локальном режиме всегда можем выполнять команды
    
    public System.Action<List<Vector2Int>> OnMoveCommandReceived { get; set; }
    public System.Action<Vector2Int> OnAttackCommandReceived { get; set; }
    Action<ulong> IUnitCommandExecutor.OnAttackCommandReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void ExecuteMoveCommand(List<Vector2Int> route)
    {
        // В локальном режиме сразу выполняем команду
        OnMoveCommandReceived?.Invoke(route);
    }
    
    public void ExecuteAttackCommand(Vector2Int targetPosition)
    {
        // В локальном режиме сразу выполняем команду
        OnAttackCommandReceived?.Invoke(targetPosition);
    }

    public void ExecuteAttackCommand(ulong targetUnitId)
    {
        throw new NotImplementedException();
    }
}
