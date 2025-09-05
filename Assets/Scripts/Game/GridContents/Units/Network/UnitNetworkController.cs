using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Game.Network;

/// <summary>
/// Сетевой контроллер юнита, отвечающий за синхронизацию команд между клиентами
/// </summary>
public class UnitNetworkController : NetworkBehaviour, IUnitNetworkController, IUnitCommandExecutor
{
    [Header("Network Settings")]
    [SerializeField] private float maxMoveDistance = 10f;
    [SerializeField] private float moveValidationTolerance = 0.1f;
    
    public bool IsOwner => NetworkObject.IsOwner;
    public bool CanExecuteCommands => IsOwner;
    
    public System.Action<List<Vector3>> OnMoveCommandReceived { get; set; }
    public System.Action<ulong> OnAttackCommandReceived { get; set; }
    
    private void Awake()
    {
        // Валидация компонентов
        if (GetComponent<UnitView3D>() == null)
        {
            Debug.LogError($"[UnitNetworkController] UnitView3D component not found on {gameObject.name}");
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"[UnitNetworkController] Network object spawned. IsOwner: {IsOwner}");
    }
    
    public void RequestMove(List<Vector3> route)
    {
        ExecuteMoveCommand(route);
    }
    
    public void ExecuteMoveCommand(List<Vector3> route)
    {
        if (!CanExecuteCommands)
        {
            Debug.LogWarning($"[UnitNetworkController] Cannot execute commands for {gameObject.name}");
            return;
        }
        
        if (route == null || route.Count == 0)
        {
            Debug.LogWarning($"[UnitNetworkController] Invalid route provided");
            return;
        }
        
        // Локальная валидация маршрута
        if (!ValidateRoute(route))
        {
            Debug.LogWarning($"[UnitNetworkController] Route validation failed");
            return;
        }
        
        MoveRequestServerRpc(route.ToArray());
    }
    
    public void RequestAttack(ulong targetUnitId)
    {
        ExecuteAttackCommand(targetUnitId);
    }
    
    public void ExecuteAttackCommand(ulong targetUnitId)
    {
        if (!CanExecuteCommands)
        {
            Debug.LogWarning($"[UnitNetworkController] Cannot execute commands for {gameObject.name}");
            return;
        }
        
        AttackRequestServerRpc(targetUnitId);
    }
    
    [ServerRpc]
    private void MoveRequestServerRpc(Vector3[] route, ServerRpcParams rpcParams = default)
    {
        // Серверная валидация
        if (!ValidateRouteOnServer(route, rpcParams.Receive.SenderClientId))
        {
            Debug.LogWarning($"[UnitNetworkController] Server validation failed for move request");
            return;
        }
        
        // Рассылаем команду всем клиентам
        MoveCommandClientRpc(route);
    }
    
    [ServerRpc]
    private void AttackRequestServerRpc(ulong targetUnitId, ServerRpcParams rpcParams = default)
    {
        // Серверная валидация атаки
        if (!ValidateAttackOnServer(targetUnitId, rpcParams.Receive.SenderClientId))
        {
            Debug.LogWarning($"[UnitNetworkController] Server validation failed for attack request");
            return;
        }
        
        AttackCommandClientRpc(targetUnitId);
    }
    
    [ClientRpc]
    private void MoveCommandClientRpc(Vector3[] route)
    {
        var routeList = new List<Vector3>(route);
        OnMoveCommandReceived?.Invoke(routeList);
    }
    
    [ClientRpc]
    private void AttackCommandClientRpc(ulong targetUnitId)
    {
        OnAttackCommandReceived?.Invoke(targetUnitId);
    }
    
    private bool ValidateRoute(List<Vector3> route)
    {
        if (route.Count == 0) return false;
        
        // Проверяем максимальную дистанцию
        float totalDistance = 0f;
        for (int i = 1; i < route.Count; i++)
        {
            totalDistance += Vector3.Distance(route[i - 1], route[i]);
        }
        
        return totalDistance <= maxMoveDistance;
    }
    
    private bool ValidateRouteOnServer(Vector3[] route, ulong clientId)
    {
        // Здесь можно добавить более сложную серверную валидацию:
        // - Проверка на читерство
        // - Проверка доступности клеток
        // - Проверка очков движения
        // - Проверка состояния юнита
        
        return ValidateRoute(new List<Vector3>(route));
    }
    
    private bool ValidateAttackOnServer(ulong targetUnitId, ulong clientId)
    {
        // Валидация атаки:
        // - Проверка дистанции до цели
        // - Проверка видимости цели
        // - Проверка состояния юнита (не мертв, не оглушен и т.д.)
        // - Проверка очков действия
        
        return true; // Заглушка
    }
}
