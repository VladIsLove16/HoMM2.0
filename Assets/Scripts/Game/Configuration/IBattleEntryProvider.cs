using UnityEngine;

/// <summary>
/// Интерфейс для получения конфигурации игры
/// Абстракция для получения настроек матча из различных источников
/// </summary>
public interface IBattleEntryProvider
{
    /// <summary>
    /// Получить выбранную конфигурацию юнитов
    /// </summary>
    /// <returns>Конфигурация юнитов или null, если не выбрана</returns>
    GridContentEntrySO GetSelectedConfiguration();
    
    /// <summary>
    /// Получить конфигурацию по индексу
    /// </summary>
    /// <param name="index">Индекс конфигурации</param>
    /// <returns>Конфигурация юнитов или null, если индекс неверный</returns>
    GridContentEntrySO GetConfigurationByIndex(int index);
    
    /// <summary>
    /// Получить индекс выбранной конфигурации
    /// </summary>
    /// <returns>Индекс выбранной конфигурации</returns>
    int GetSelectedConfigurationIndex();

    public bool AcceptStartingBattleWithoutClients { get; }
}

