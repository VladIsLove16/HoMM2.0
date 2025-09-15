using UnityEngine;
using Zenject;

/// <summary>
/// Провайдер конфигурации для игровой сцены
/// Получает данные из SceneTransitionDataService
/// Не зависит от объектов из других сцен
/// </summary>
public class GameSceneConfigurationProvider : IGameConfigurationProvider, IInitializable
{
    private readonly SceneTransitionDataService _dataService;

    bool IGameConfigurationProvider.AcceptStartingBattleWithoutClients => throw new System.NotImplementedException();

    [Inject]
    public GameSceneConfigurationProvider(SceneTransitionDataService dataService)
    {
        _dataService = dataService;
    }
    
    public void Initialize()
    {
        if (_dataService != null)
        {
            // Подписываемся на изменения конфигурации
            _dataService.OnConfigurationChanged += OnConfigurationChanged;
            Debug.Log("[GameSceneConfigurationProvider] Initialized with SceneTransitionDataService");
            
            // Принудительно загружаем конфигурации
            _dataService.ReloadConfigurations();
        }
        else
        {
            Debug.LogWarning("[GameSceneConfigurationProvider] SceneTransitionDataService is null");
        }
        
        // Проверяем доступность конфигураций
        var availableConfigs = _dataService?.GetAvailableConfigurations();
        if (availableConfigs != null && availableConfigs.Length > 0)
        {
            Debug.Log($"[GameSceneConfigurationProvider] Available configurations: {availableConfigs.Length}");
        }
        else
        {
            Debug.LogWarning("[GameSceneConfigurationProvider] No available configurations found - this might be normal if LobbySceneInitializer hasn't run yet");
        }
    }
    
    private void OnConfigurationChanged(int newIndex)
    {
        Debug.Log($"[GameSceneConfigurationProvider] Configuration changed to index: {newIndex}");
    }
    
    public GridContentEntrySO GetSelectedConfiguration()
    {
        if (_dataService == null)
        {
            Debug.LogWarning("[GameSceneConfigurationProvider] DataService is null");
            return null;
        }
        
        var config = _dataService.GetSelectedConfiguration();
        Debug.Log($"[GameSceneConfigurationProvider] GetSelectedConfiguration: {(config != null ? config.name : "null")}");
        return config;
    }
    
    public GridContentEntrySO GetConfigurationByIndex(int index)
    {
        if (_dataService == null)
        {
            Debug.LogWarning("[GameSceneConfigurationProvider] DataService is null");
            return null;
        }
        
        return _dataService.GetConfigurationByIndex(index);
    }
    
    public int GetSelectedConfigurationIndex()
    {
        if (_dataService == null)
        {
            Debug.LogWarning("[GameSceneConfigurationProvider] DataService is null");
            return 0;
        }
        
        return _dataService.GetSelectedConfigurationIndex();
    }

    public bool AcceptStartingBattleWithoutClients()
    {
        return _dataService.AcceptStartingGameWithoutClients;
    }
}
