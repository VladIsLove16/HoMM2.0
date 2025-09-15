
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Game.Network;

public enum UnitType
{
    Archer,
    Witch,
    Warrok
}

/// <summary>
/// Фабрика для создания юнитов в зависимости от режима игры
/// </summary>
public class UnitViewFactory
{
    readonly DiContainer _container;
    [Inject] IReadOnlyDictionary<UnitType, UnitDefinitionSO> _dataMap;
    readonly Transform _unitsParent;
    readonly IWorldToCellProvider _worldToCellProvider;
    readonly UnitPrefabManager _prefabManager;
    
    [Inject]
    public UnitViewFactory(
        DiContainer container,
        IWorldToCellProvider worldToCellProvider,
        UnitPrefabManager prefabManager,
        [Inject(Id = "UnitsParent")] Transform unitsParent)
    {
        _container = container;
        _unitsParent = unitsParent;
        _worldToCellProvider = worldToCellProvider;
        _prefabManager = prefabManager;
    }

    /// <summary>
    /// Создаёт UnitView3D в зависимости от режима игры
    /// </summary>
    public virtual UnitView3D Create(IViewModel viewModel)
    {
        var unitVM = viewModel as UnitViewModel;
        if (unitVM == null)
        {
            Debug.LogError("[UnitViewFactory] ViewModel is not UnitViewModel");
            return null;
        }
        var model = unitVM.Model;
        var unitType = model.UnitType.Value;

        // Берём префаб по типу и текущему режиму (может быть один и тот же префаб для обоих режимов)
        var prefab = _prefabManager.GetPrefab(unitType);
        if (prefab == null)
        {
            Debug.LogError($"[UnitViewFactory] Prefab not found for unit type: {unitType}");
            return null;
        }

        Vector3 worldPos = _worldToCellProvider.ToWorld(model.Position.Value.x, model.Position.Value.y);
        var view = _container
            .InstantiatePrefabForComponent<UnitView3D>(
                prefab,
                worldPos,
                Quaternion.identity,
                _unitsParent
            );

        view.Init(unitVM);
        return view;
    }
}

//public class UnitViewFactoryDebugger : UnitViewFactory
//{
//    public UnitViewFactoryDebugger(DiContainer container, Dictionary<UnitType, UnitDefinitionSO> allUnitDatas, IWorldToCellProvider worldToCellProvider, [Inject(Id = "UnitsParent")] Transform unitsParent) : base(container , worldToCellProvider, unitsParent)
//    {
//        Debug.Log("UnitViewFactory is ready");
//    }

//    public override UnitView3D Create(IViewModel viewModel)
//    {
//        var unitViewModel = viewModel as UnitViewModel;
//        UnitModel model = unitViewModel.Model;
//        Debug.Log($"UnitView3D {model.UnitType} creating in  {model.Position.Value.x} {model.Position.Value.y}");
//        return base.Create(unitViewModel);
//    }
//}