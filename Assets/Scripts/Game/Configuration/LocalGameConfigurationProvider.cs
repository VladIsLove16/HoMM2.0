using UnityEngine;
using Zenject;

/// <summary>
/// Локальный провайдер конфигурации игры
/// Используется для локального тестирования без сети
/// </summary>
public class LocalGameConfigurationProvider : IGameConfigurationProvider, IInitializable
{
    private readonly GridContentEntrySO[] _availableConfigs;
    private int _selectedIndex = 0;

    bool IGameConfigurationProvider.AcceptStartingBattleWithoutClients { get
        {
            return true;
        } }

    [Inject]
    public LocalGameConfigurationProvider([Inject(Id = "AvailableConfigs")] GridContentEntrySO[] availableConfigs)
    {
        _availableConfigs = availableConfigs;
    }
    
    public void Initialize()
    {
        Debug.Log("[LocalGameConfigurationProvider] Initialized for local testing");
    }
    
    public GridContentEntrySO GetSelectedConfiguration()
    {
        return GetConfigurationByIndex(_selectedIndex);
    }
    
    public GridContentEntrySO GetConfigurationByIndex(int index)
    {
        if (_availableConfigs == null || index < 0 || index >= _availableConfigs.Length)
        {
            Debug.LogWarning($"[LocalGameConfigurationProvider] Invalid config index: {index}");
            return GetDefaultConfiguration();
        }
        
        return _availableConfigs[index];
    }
    
    public int GetSelectedConfigurationIndex()
    {
        return _selectedIndex;
    }
    
    /// <summary>
    /// Установить выбранную конфигурацию (только для локального режима)
    /// </summary>
    public void SetSelectedConfiguration(int index)
    {
        if (_availableConfigs != null && index >= 0 && index < _availableConfigs.Length)
        {
            _selectedIndex = index;
            Debug.Log($"[LocalGameConfigurationProvider] Selected configuration: {index}");
        }
    }
    
    private GridContentEntrySO GetDefaultConfiguration()
    {
        return _availableConfigs != null && _availableConfigs.Length > 0 ? _availableConfigs[0] : null;
    }
}

