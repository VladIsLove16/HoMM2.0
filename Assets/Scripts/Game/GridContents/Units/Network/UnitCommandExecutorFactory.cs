using UnityEngine;
using Game.Network;
using Zenject;

/// <summary>
/// Фабрика для создания исполнителей команд юнитов в зависимости от режима игры
/// </summary>
public class UnitCommandExecutorFactory : IUnitCommandExecutorFactory
{
    /// <summary>
    /// Создает исполнитель команд для юнита в зависимости от режима игры
    /// </summary>
    /// <param name="unitObject">GameObject юнита</param>
    /// <param name="gameMode">Режим игры (Singleplayer/Multiplayer)</param>
    /// <returns>Исполнитель команд</returns>
    public IUnitCommandExecutor CreateExecutor(GameObject unitObject, GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.Singleplayer:
                return CreateLocalExecutor(unitObject);
                
            case GameMode.Multiplayer:
                return CreateNetworkExecutor(unitObject);
                
            default:
                Debug.LogError($"[UnitCommandExecutorFactory] Unknown game mode: {gameMode}");
                return CreateLocalExecutor(unitObject); // Fallback
        }
    }
    
    /// <summary>
    /// Создает локальный исполнитель команд
    /// </summary>
    private static IUnitCommandExecutor CreateLocalExecutor(GameObject unitObject)
    {
        var executor = unitObject.GetComponent<LocalUnitCommandExecutor>();
        if (executor == null)
        {
            executor = unitObject.AddComponent<LocalUnitCommandExecutor>();
        }
        
        // Удаляем сетевой компонент, если он есть
        var networkController = unitObject.GetComponent<UnitNetworkController>();
        if (networkController != null)
        {
            Object.Destroy(networkController);
        }
        
        return executor;
    }
    
    /// <summary>
    /// Создает сетевой исполнитель команд
    /// </summary>
    private static IUnitCommandExecutor CreateNetworkExecutor(GameObject unitObject)
    {
        var executor = unitObject.GetComponent<UnitNetworkController>();
        if (executor == null)
        {
            executor = unitObject.AddComponent<UnitNetworkController>();
        }
        
        // Удаляем локальный компонент, если он есть
        var localExecutor = unitObject.GetComponent<LocalUnitCommandExecutor>();
        if (localExecutor != null)
        {
            Object.Destroy(localExecutor);
        }
        
        return executor;
    }
    
    /// <summary>
    /// Получает существующий исполнитель команд или создает новый
    /// </summary>
    public IUnitCommandExecutor GetOrCreateExecutor(GameObject unitObject, GameMode gameMode)
    {
        var existingExecutor = unitObject.GetComponent<IUnitCommandExecutor>();
        if (existingExecutor != null)
        {
            return existingExecutor;
        }
        
        return CreateExecutor(unitObject, gameMode);
    }
}

/// <summary>
/// Режимы игры
/// </summary>
public enum GameMode
{
    Singleplayer,
    Multiplayer
}
