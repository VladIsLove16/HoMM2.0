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
    public bool CanExecuteCommands => IsOwner;
    // DI: доменная модель и провайдер координат
    [Zenject.Inject] private GameModel _gameModel;
    [Zenject.Inject] private IWorldToCellProvider _worldToCellProvider;
    
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
        Debug.Log("ExecuteMoveCommand");
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
        Debug.Log("MoveRequestServerRpc");
        // Серверная валидация
        if (!ValidateRouteOnServer(route, rpcParams.Receive.SenderClientId))
        {
            Debug.LogWarning($"[UnitNetworkController] Server validation failed for move request");
            return;
        }
        
        try
        {
            var unitView = GetComponent<UnitView3D>();
            if (unitView == null || unitView.Model == null)
            {
                Debug.LogError("[UnitNetworkController] UnitView3D/Model is null on server");
                return;
            }
            
            if (_gameModel == null || _worldToCellProvider == null)
            {
                Debug.LogError("[UnitNetworkController] _gameModel or _worldToCellProvider not injected on server");
                return;
            }
            
            // Конвертируем маршрут в координаты сетки
            var gridRoute = new List<UnityEngine.Vector2Int>(route.Length);
            foreach (var wp in route)
            {
                if (_worldToCellProvider.ToGrid(wp, out var cell))
                {
                    gridRoute.Add(new UnityEngine.Vector2Int(cell.x, cell.y));
                }
            }
            
            // Обновляем модель на сервере
            _gameModel.MoveObject(unitView.Model, gridRoute);
            Debug.Log($"[UnitNetworkController] Server moved unit {unitView.Model.UnitType} to {gridRoute[gridRoute.Count - 1]}");
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            return;
        }

        // Рассылаем команду всем клиентам для синхронизации анимации
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
        // Проверяем, что это не владелец объекта (он уже выполнил команду локально)
        if (IsOwner)
        {
            Debug.Log($"[UnitNetworkController] Skipping move command for owner");
            return;
        }
        
        var routeList = new List<Vector3>(route);
        Debug.Log($"[UnitNetworkController] Received move command for non-owner, route points: {routeList.Count}");
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
