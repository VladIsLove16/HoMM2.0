
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using static UnityEditor.Profiling.HierarchyFrameDataView;
public enum UnitType
{
    Archer,
    Witch,
    Warrior
}
public class UnitViewFactory
{
    readonly DiContainer _container;
    readonly CombatController _combatController;
    readonly Dictionary<UnitType, UnitDefinitionSO> _dataMap;
    readonly Transform _unitsParent;
    readonly ICellGridRenderer _gridRenderer;
    // Внедряем все SO-объекты через Zenject
    public UnitViewFactory(
        DiContainer container,
        IEnumerable<UnitDefinitionSO> allUnitDatas,
        ICellGridRenderer gridRenderer,
        CombatController combatController,
        [Inject(Id = "UnitsParent")] Transform unitsParent)
    {
        _container = container;
        _combatController = combatController;
        _unitsParent = unitsParent;
        _gridRenderer = gridRenderer;
        // Собираем словарь по enum’у
        _dataMap = allUnitDatas.ToDictionary(d => d.UnitType);
    }

    /// <summary>
    /// СоздаётUnitView3D (префаб) на позиции (x,y).
    /// </summary>
    public UnitView3D Create(UnitViewModel viewModel)
    {
        if (!_dataMap.TryGetValue(viewModel.UnitType, out var data))
            throw new KeyNotFoundException($"Нет UnitDefinitionSO для типа {viewModel.UnitType}");

        Vector3 worldPos = _gridRenderer.ToWorld(viewModel.X, viewModel.Y);

        var view = _container
            .InstantiatePrefabForComponent<UnitView3D>(
                data.UnitViewPrefab,
                worldPos,
                Quaternion.identity,
                _unitsParent
            );
        Debug.Log($"UnitView3D {viewModel.UnitType} created {viewModel.X} {viewModel.Y}");
        return view;
    }
}

