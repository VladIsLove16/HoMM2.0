using System.Collections.Generic;
using UnityEngine;

namespace Game.Network
{
    /// <summary>
    /// Интерфейс для сетевого управления юнитом
    /// </summary>
    public interface IUnitNetworkController
    {
        /// <summary>
        /// Запрос на перемещение юнита по маршруту
        /// </summary>
        /// <param name="route">Маршрут перемещения в мировых координатах</param>
        void RequestMove(List<Vector3> route);
        
        /// <summary>
        /// Запрос на атаку цели
        /// </summary>
        /// <param name="targetUnitId">ID цели</param>
        void RequestAttack(ulong targetUnitId);
        
        /// <summary>
        /// Проверяет, является ли текущий клиент владельцем юнита
        /// </summary>
        bool IsOwner { get; }
        
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
