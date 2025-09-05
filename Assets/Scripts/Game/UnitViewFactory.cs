
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
    readonly GameModeManager _gameModeManager;
    readonly NetworkUnitViewFactory _networkFactory;
    
    public UnitViewFactory(
        DiContainer container,
        IWorldToCellProvider worldToCellProvider,
        GameModeManager gameModeManager,
        NetworkUnitViewFactory networkFactory,
        [Inject(Id = "UnitsParent")] Transform unitsParent)
    {
        _container = container;
        _unitsParent = unitsParent;
        _worldToCellProvider = worldToCellProvider;
        _gameModeManager = gameModeManager;
        _networkFactory = networkFactory;
    }

    public UnitViewFactory(DiContainer container, IWorldToCellProvider worldToCellProvider, Transform unitsParent)
    {
        _container = container;
        _worldToCellProvider = worldToCellProvider;
        _unitsParent = unitsParent;
    }

    /// <summary>
    /// Создаёт UnitView3D в зависимости от режима игры
    /// </summary>
    public virtual UnitView3D Create(IViewModel viewModel)
    {
        return _networkFactory.CreateUnit(viewModel);
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