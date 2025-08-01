
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using static UnityEditor.Profiling.HierarchyFrameDataView;
public enum UnitType
{
    Archer,
    Witch,
    Warrok
}
public class UnitViewFactory
{
    readonly DiContainer _container;
    [Inject] IReadOnlyDictionary<UnitType, UnitDefinitionSO> _dataMap;
    readonly Transform _unitsParent;
    readonly IWorldToCellProvider _worldToCellProvider;
    public UnitViewFactory(
        DiContainer container,
        IWorldToCellProvider worldToCellProvider,
        [Inject(Id = "UnitsParent")] Transform unitsParent)
    {
        _container = container;
        _unitsParent = unitsParent;
        _worldToCellProvider = worldToCellProvider;
    }

    /// <summary>
    /// СоздаётUnitView3D (префаб) на позиции (x,y).
    /// </summary>
    public virtual UnitView3D Create(IViewModel viewModel)
    {
        var unitViewModel = viewModel as UnitViewModel;
        UnitType unitType = unitViewModel.Model.UnitType.Value;
        UnitModel model = unitViewModel.Model;
        if (!_dataMap.TryGetValue(unitType, out var data))
        {
            Debug.LogError($"Нет UnitDefinitionSO для типа {unitType}");
            return null;
        }

        Vector3 worldPos = _worldToCellProvider.ToWorld(model.Position.Value.x, model.Position.Value.y);

        var view = _container
            .InstantiatePrefabForComponent<UnitView3D>(
                data.UnitViewPrefab,
                worldPos,
                Quaternion.identity,
                _unitsParent
            );
        view.Init(unitViewModel);
        return view;
    }
}

public class UnitViewFactoryDebugger : UnitViewFactory
{
    public UnitViewFactoryDebugger(DiContainer container, Dictionary<UnitType, UnitDefinitionSO> allUnitDatas, IWorldToCellProvider worldToCellProvider, [Inject(Id = "UnitsParent")] Transform unitsParent) : base(container , worldToCellProvider, unitsParent)
    {
        Debug.Log("UnitViewFactory is ready");
    }

    public override UnitView3D Create(IViewModel viewModel)
    {
        var unitViewModel = viewModel as UnitViewModel;
        UnitModel model = unitViewModel.Model;
        Debug.Log($"UnitView3D {model.UnitType} creating in  {model.Position.Value.x} {model.Position.Value.y}");
        return base.Create(unitViewModel);
    }
}