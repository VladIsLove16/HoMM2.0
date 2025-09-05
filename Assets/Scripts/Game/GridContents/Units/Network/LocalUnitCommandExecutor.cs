using System.Collections.Generic;
using UnityEngine;
using Game.Network;

/// <summary>
/// Локальный исполнитель команд для одиночной игры
/// </summary>
public class LocalUnitCommandExecutor : MonoBehaviour, IUnitCommandExecutor
{
    public bool CanExecuteCommands => true; // В локальном режиме всегда можем выполнять команды
    
    public System.Action<List<Vector3>> OnMoveCommandReceived { get; set; }
    public System.Action<ulong> OnAttackCommandReceived { get; set; }
    
    public void ExecuteMoveCommand(List<Vector3> route)
    {
        // В локальном режиме сразу выполняем команду
        OnMoveCommandReceived?.Invoke(route);
    }
    
    public void ExecuteAttackCommand(ulong targetUnitId)
    {
        // В локальном режиме сразу выполняем команду
        OnAttackCommandReceived?.Invoke(targetUnitId);
    }
}
