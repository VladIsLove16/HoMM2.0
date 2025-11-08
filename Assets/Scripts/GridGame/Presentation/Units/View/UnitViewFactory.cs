using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

/// <summary>
/// Фабрика для создания UnitView3D в клеточном бою.
/// </summary>
public class UnitViewFactory
{
    private readonly DiContainer _container;
    private readonly IUnitViewDefinition<UnitView3D> _presentations;
    private readonly Transform _unitsParent;
    private readonly IWorldToCellProvider _worldToCellProvider;

    [Inject]
    public UnitViewFactory(
        DiContainer container,
        IWorldToCellProvider worldToCellProvider,
        IUnitViewDefinition<UnitView3D> presentationMap,
        [Inject(Id = "UnitsParent")] Transform unitsParent)
    {
        _container = container;
        _worldToCellProvider = worldToCellProvider;
        _presentations = presentationMap;
        _unitsParent = unitsParent;
    }

    /// <summary>
    /// Создаёт UnitView3D на сетке боя.
    /// </summary>
    public virtual UnitView3D Create(IViewModel viewModel)
    {
        if (viewModel is not UnitViewModel unitVM)
        {
            Debug.LogError("[UnitViewFactory] ViewModel is not UnitViewModel");
            return null;
        }

        var model = unitVM.Model;
        var unitType = model.UnitType.Value;

        if (!_presentations.TryGetAsset(unitType, out var prefab) || prefab == null)
        {
            Debug.LogWarning($"[UnitViewFactory] Grid presentation not found for unit type: {unitType}.");
            return null;
        }

        Vector3 worldPos = _worldToCellProvider.ToWorld(model.Position.Value.x, model.Position.Value.y);

        var view = _container.InstantiatePrefabForComponent<UnitView3D>(
            prefab,
            worldPos,
            Quaternion.identity,
            _unitsParent);
        view.gameObject.transform.localScale = Vector3.one;
        view.Init(unitVM);
        return view;
    }
}


