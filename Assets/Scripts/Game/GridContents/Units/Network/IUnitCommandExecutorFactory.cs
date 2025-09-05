using System.Collections.Generic;
using UnityEngine;
using Game.Network;

namespace Game.Network
{
    /// <summary>
    /// Интерфейс фабрики для создания исполнителей команд юнитов
    /// </summary>
    public interface IUnitCommandExecutorFactory
    {
        /// <summary>
        /// Создает исполнитель команд для юнита в зависимости от режима игры
        /// </summary>
        IUnitCommandExecutor CreateExecutor(GameObject unitObject, GameMode gameMode);
        
        /// <summary>
        /// Получает существующий исполнитель команд или создает новый
        /// </summary>
        IUnitCommandExecutor GetOrCreateExecutor(GameObject unitObject, GameMode gameMode);
    }
}
