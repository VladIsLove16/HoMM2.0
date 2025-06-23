
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
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
        Debug.Log("UnitViewFactory is ready");
    }

    /// <summary>
    /// СоздаётUnitView3D (префаб) на позиции (x,y).
    /// </summary>
    public UnitView3D Create(UnitViewModel viewModel)
    {
        if (!_dataMap.TryGetValue(viewModel.UnitType, out var data))
        {
            Debug.LogError($"Нет UnitDefinitionSO для типа {viewModel.UnitType}");
            return null;
        }

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

