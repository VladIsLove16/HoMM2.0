using System.Collections.Generic;
using UnityEngine;

namespace Game.Network
{
    /// <summary>
    /// Интерфейс для выполнения команд юнита (локально или через сеть)
    /// </summary>
    public interface IUnitCommandExecutor
    {
        /// <summary>
        /// Выполняет команду перемещения
        /// </summary>
        void ExecuteMoveCommand(List<Vector3> route);
        
        /// <summary>
        /// Выполняет команду атаки
        /// </summary>
        void ExecuteAttackCommand(ulong targetUnitId);
        
        /// <summary>
        /// Проверяет, может ли текущий клиент выполнять команды для этого юнита
        /// </summary>
        bool CanExecuteCommands { get; }
        
        /// <summary>
        /// Событие, вызываемое при получении команды на перемещение
        /// </summary>
        System.Action<List<Vector3>> OnMoveCommandReceived { get; set; }
        
        /// <summary>
        /// Событие, вызываемое при получении команды на атаку
        /// </summary>
        System.Action<ulong> OnAttackCommandReceived { get; set; }
    }
}
