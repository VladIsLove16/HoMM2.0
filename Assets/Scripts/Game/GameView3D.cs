// GameView3D.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class GameView3D : MonoBehaviour
{
    private UnitViewFactory _factory;
    private Dictionary<IViewModel, UnitView3D> _views = new();
    private Dictionary<UnitModel, UnitView3D> _models = new();
    private GameViewModel _gameVM;
    IGridCellRenderer _renderer;
    [Inject]
    public void Construct(GameViewModel gameVM, IGridCellRenderer renderer, UnitViewFactory unitViewFactory)
    {
        Debug.Log("GameView3D inject");

        _gameVM = gameVM;
        _renderer = renderer;
        _factory = unitViewFactory;

        _gameVM.UnitSpawned += OnUnitSpawned;
        _gameVM.UnitRemoved += OnUnitRemoved;
        _gameVM.UnitMovedByRoute += OnUnitMovedByRoute;
    }

    private void OnUnitMovedByRoute(IViewModel viewModel, List<Vector3> route)
    {
        var view = _views[viewModel];
        view.MoveByRoute(route);
    }

    public virtual void OnUnitSpawned(IViewModel viewModel)
    {
        var view = _factory.Create(viewModel);
        _views[viewModel] = view;
        if(viewModel is UnitViewModel unitVM)
        {
            UnitModel unitModel = unitVM.Model;
            _models[unitModel] = view;
        }
    }

    public void OnUnitRemoved(IViewModel viewModel)
    {
        if (_views.TryGetValue(viewModel, out var view))
        {
            _views.Remove(viewModel);
        }
    }
    public UnitView3D GetView(UnitModel model)
    {
        return _models[model];
    }
}