
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
public class UnitFactory
{
    readonly DiContainer _container;
    readonly CombatController _combatController;
    readonly Dictionary<UnitType, UnitDataSO> _dataMap;
    readonly Transform _unitsParent;
    readonly ICellGridRenderer _gridRenderer;
    // Внедряем все SO-объекты через Zenject
    public UnitFactory(
        DiContainer container,
        IEnumerable<UnitDataSO> allUnitDatas,
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

    public (UnitViewModel vm, UnitView view) Create(UnitType unitType, int amount, int x, int y)
    {
        if (!_dataMap.TryGetValue(unitType, out var data))
            throw new KeyNotFoundException($"No UnitData for type {unitType}");

        var vm = new UnitViewModel(data, amount, _combatController);

        var pos = _gridRenderer.ToWorld(x,y);
        var view = _container.InstantiatePrefabForComponent<UnitView>(
            data.UnitViewPrefab, pos, Quaternion.identity, _unitsParent);

        view.Construct(vm);

        return (vm, view);
    }
}

