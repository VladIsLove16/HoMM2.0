using System.Collections.Generic;
using UnityEngine;
using Game.Network;
using Zenject;

/// <summary>
/// Сервис для управления сетевыми юнитами
/// </summary>
public class UnitNetworkService : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private bool enableNetworkValidation = true;
    [SerializeField] private float maxMoveDistance = 10f;
    
    
    /// <summary>
    /// Получает сетевой контроллер для юнита
    /// </summary>
    public IUnitNetworkController GetNetworkController(GameObject unitObject)
    {
        return unitObject.GetComponent<IUnitNetworkController>();
    }
    
    /// <summary>
    /// Проверяет, является ли юнит сетевым
    /// </summary>
    public bool IsNetworkUnit(GameObject unitObject)
    {
        return unitObject.GetComponent<IUnitNetworkController>() != null;
    }
    
    /// <summary>
    /// Валидирует маршрут перемещения
    /// </summary>
    public bool ValidateMoveRoute(List<Vector3> route)
    {
        if (!enableNetworkValidation) return true;
        
        if (route == null || route.Count == 0) return false;
        
        float totalDistance = 0f;
        for (int i = 1; i < route.Count; i++)
        {
            totalDistance += Vector3.Distance(route[i - 1], route[i]);
        }
        
        return totalDistance <= maxMoveDistance;
    }
}
