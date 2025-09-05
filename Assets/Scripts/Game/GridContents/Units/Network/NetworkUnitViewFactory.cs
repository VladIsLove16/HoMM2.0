using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Game.Network;

/// <summary>
/// Фабрика для создания юнитов с правильными префабами
/// </summary>
public class NetworkUnitViewFactory
{
    readonly DiContainer _container;
    readonly Transform _unitsParent;
    readonly IWorldToCellProvider _worldToCellProvider;
    readonly GameModeManager _gameModeManager;
    readonly UnitPrefabManager _prefabManager;
    [Inject] IReadOnlyDictionary<UnitType, UnitDefinitionSO> _dataMap;
    
    public NetworkUnitViewFactory(
        DiContainer container,
        IWorldToCellProvider worldToCellProvider,
        GameModeManager gameModeManager,
        UnitPrefabManager prefabManager,
        [Inject(Id = "UnitsParent")] Transform unitsParent)
    {
        _container = container;
        _unitsParent = unitsParent;
        _worldToCellProvider = worldToCellProvider;
        _gameModeManager = gameModeManager;
        _prefabManager = prefabManager;
    }

    /// <summary>
    /// Создает UnitView3D с правильным префабом для режима игры
    /// </summary>
    public virtual UnitView3D CreateUnit(IViewModel viewModel)
    {
        var unitViewModel = viewModel as UnitViewModel;
        UnitType unitType = unitViewModel.Model.UnitType.Value;
        UnitModel model = unitViewModel.Model;
        
        // Получаем данные юнита из единого источника
        if (!_dataMap.TryGetValue(unitType, out var unitData))
        {
            Debug.LogError($"[NetworkUnitViewFactory] UnitDefinitionSO not found for unit type: {unitType}");
            return null;
        }
        
        // Валидируем конфигурацию
        if (!unitData.IsValid())
        {
            Debug.LogError($"[NetworkUnitViewFactory] UnitDefinitionSO is not valid for unit type: {unitType}");
            return null;
        }
        
        // Получаем правильный префаб для режима игры
        var prefab = _prefabManager.GetPrefab(unitType, _gameModeManager.CurrentGameMode);
        if (prefab == null)
        {
            Debug.LogError($"[NetworkUnitViewFactory] Prefab not found for unit type: {unitType}, mode: {_gameModeManager.CurrentGameMode}");
            return null;
        }

        Vector3 worldPos = _worldToCellProvider.ToWorld(model.Position.Value.x, model.Position.Value.y);

        // Создаем юнит из правильного префаба
        var view = _container
            .InstantiatePrefabForComponent<UnitView3D>(
                prefab,
                worldPos,
                Quaternion.identity,
                _unitsParent
            );
        
        // Настраиваем юнит
        view.Init(unitViewModel);
        
        // Для сетевых юнитов спавним через NetworkManager
        if (_gameModeManager.CurrentGameMode == GameMode.Multiplayer)
        {
            var networkObject = view.GetComponent<NetworkObject>();
            if (networkObject != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                networkObject.Spawn();
            }
        }
        
        return view;
    }
    
    /// <summary>
    /// Создает сетевой UnitView3D (для обратной совместимости)
    /// </summary>
    public virtual UnitView3D CreateNetworkUnit(IViewModel viewModel)
    {
        return CreateUnit(viewModel);
    }
    
    /// <summary>
    /// Создает локальный UnitView3D (для обратной совместимости)
    /// </summary>
    public virtual UnitView3D CreateLocalUnit(IViewModel viewModel)
    {
        return CreateUnit(viewModel);
    }
}
