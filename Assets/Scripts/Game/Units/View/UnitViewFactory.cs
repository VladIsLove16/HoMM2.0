
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
    readonly IGridCellRenderer _gridRenderer;
    public UnitViewFactory(
        DiContainer container,
        IEnumerable<UnitDefinitionSO> allUnitDatas,
        IGridCellRenderer gridRenderer,
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
    public UnitView3D Create(UnitModel model)
    {
        if (!_dataMap.TryGetValue(model.BaseUnitStats.UnitType, out var data))
        {
            Debug.LogError($"Нет UnitDefinitionSO для типа {model.UnitType}");
            return null;
        }

        Vector3 worldPos = _gridRenderer.ToWorld(model.Position.Value.x, model.Position.Value.y);

        var view = _container
            .InstantiatePrefabForComponent<UnitView3D>(
                data.UnitViewPrefab,
                worldPos,
                Quaternion.identity,
                _unitsParent
            );
        view.SetMaterial(model.IsBlueTeam.Value ? _dataMap[model.BaseUnitStats.UnitType].BlueTeamMaterial : _dataMap[model.BaseUnitStats.UnitType].RedTeamMaterial);

        Debug.Log($"UnitView3D {model.UnitType} created {model.Position.Value.x} {model.Position.Value.y}");
        return view;
    }
}

