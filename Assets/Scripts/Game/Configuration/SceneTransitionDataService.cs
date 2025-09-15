using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Сервис для передачи данных между сценами
/// Реализует паттерн Singleton для глобального доступа
/// Не уничтожается при смене сцен
/// </summary>
public class SceneTransitionDataService : MonoBehaviour, IGameModeProvider
{
    private static SceneTransitionDataService _instance;
    public static SceneTransitionDataService Instance => _instance;
    
    [Header("Game Configuration")]
    [SerializeField] private GridContentEntrySO[] _availableConfigs = new GridContentEntrySO[0];
    [SerializeField] private bool acceptStartingGameWithoutClients;
    public bool AcceptStartingGameWithoutClients => acceptStartingGameWithoutClients;
    public GameMode CurrentGameMode { get; private set; }
    
    private int _selectedConfigIndex = 0;
    private Dictionary<string, object> _transitionData = new Dictionary<string, object>();
    
    /// <summary>
    /// Событие изменения выбранной конфигурации
    /// </summary>
    public event Action<int> OnConfigurationChanged;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Загружаем конфигурации из сохраненных данных
            LoadConfigurationsFromData();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadConfigurationsFromData()
    {
        var savedConfigs = GetTransitionData<GridContentEntrySO[]>("AvailableConfigs");
        if (savedConfigs != null)
        {
            _availableConfigs = savedConfigs;
            Debug.Log($"[SceneTransitionDataService] Loaded {savedConfigs.Length} configurations from transition data");
        }
        else
        {
            Debug.Log("[SceneTransitionDataService] No configurations found in transition data yet - waiting for LobbySceneInitializer");
        }
    }
    
    /// <summary>
    /// Установить выбранную конфигурацию
    /// </summary>
    public void SetSelectedConfiguration(int configIndex)
    {
        if (_availableConfigs != null && configIndex >= 0 && configIndex < _availableConfigs.Length)
        {
            _selectedConfigIndex = configIndex;
            OnConfigurationChanged?.Invoke(configIndex);
            Debug.Log($"[SceneTransitionDataService] Configuration set to index: {configIndex}");
        }
        else
        {
            Debug.LogWarning($"[SceneTransitionDataService] Invalid config index: {configIndex}");
        }
    }
    public void SetGameMode(GameMode gameMode)
    {
        CurrentGameMode = gameMode;
    }
    /// <summary>
    /// Получить выбранную конфигурацию
    /// </summary>
    public GridContentEntrySO GetSelectedConfiguration()
    {
        return GetConfigurationByIndex(_selectedConfigIndex);
    }
    
    /// <summary>
    /// Получить конфигурацию по индексу
    /// </summary>
    public GridContentEntrySO GetConfigurationByIndex(int index)
    {
        if (_availableConfigs != null && index >= 0 && index < _availableConfigs.Length)
        {
            return _availableConfigs[index];
        }
        
        Debug.LogWarning($"[SceneTransitionDataService] Invalid config index: {index}");
        return GetDefaultConfiguration();
    }
    
    /// <summary>
    /// Получить индекс выбранной конфигурации
    /// </summary>
    public int GetSelectedConfigurationIndex()
    {
        return _selectedConfigIndex;
    }
    
    /// <summary>
    /// Получить все доступные конфигурации
    /// </summary>
    public GridContentEntrySO[] GetAvailableConfigurations()
    {
        return _availableConfigs;
    }
    
    /// <summary>
    /// Принудительно загрузить конфигурации из переходных данных
    /// </summary>
    public void ReloadConfigurations()
    {
        LoadConfigurationsFromData();
    }
    
    /// <summary>
    /// Сохранить данные для передачи между сценами
    /// </summary>
    public void SetTransitionData(string key, object value)
    {
        _transitionData[key] = value;
    }
    
    /// <summary>
    /// Получить данные, переданные между сценами
    /// </summary>
    public T GetTransitionData<T>(string key, T defaultValue = default(T))
    {
        if (_transitionData.TryGetValue(key, out var value) && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }
    
    private GridContentEntrySO GetDefaultConfiguration()
    {
        return _availableConfigs != null && _availableConfigs.Length > 0 ? _availableConfigs[0] : null;
    }
}
