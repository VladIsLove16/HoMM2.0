using System.Collections.Generic;
using UnityEngine;
using Game.Network;
using Zenject;

/// <summary>
/// Сервис для управления исполнителями команд юнитов
/// </summary>
public class UnitCommandExecutorService
{
    private readonly IUnitCommandExecutorFactory _executorFactory;
    private readonly GameModeManager _gameModeManager;
    
    [Inject]
    public UnitCommandExecutorService(
        IUnitCommandExecutorFactory executorFactory,
        GameModeManager gameModeManager)
    {
        _executorFactory = executorFactory;
        _gameModeManager = gameModeManager;
    }
    
    /// <summary>
    /// Настраивает исполнителя команд для юнита
    /// </summary>
    public void SetupUnit(GameObject unitObject)
    {
        var executor = _executorFactory.GetOrCreateExecutor(unitObject, _gameModeManager.CurrentGameMode);
        Debug.Log($"[UnitCommandExecutorService] Setup unit {unitObject.name} with {executor.GetType().Name}");
    }
    
    /// <summary>
    /// Настраивает всех юнитов на сцене
    /// </summary>
    public void SetupAllUnits()
    {
        var unitViews = Object.FindObjectsOfType<UnitView3D>();
        foreach (var unitView in unitViews)
        {
            SetupUnit(unitView.gameObject);
        }
        
        Debug.Log($"[UnitCommandExecutorService] Setup {unitViews.Length} units for {_gameModeManager.CurrentGameMode} mode");
    }
    
    /// <summary>
    /// Получает исполнитель команд для юнита
    /// </summary>
    public IUnitCommandExecutor GetExecutor(GameObject unitObject)
    {
        return unitObject.GetComponent<IUnitCommandExecutor>();
    }
    
    /// <summary>
    /// Проверяет, может ли юнит выполнять команды
    /// </summary>
    public bool CanExecuteCommands(GameObject unitObject)
    {
        var executor = GetExecutor(unitObject);
        return executor?.CanExecuteCommands ?? false;
    }
}
