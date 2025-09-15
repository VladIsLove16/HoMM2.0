// GameView3D.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public interface IUnitViewResolver
{
    bool TryGetView(UnitModel model, out UnitView3D view);
}

public class GameView3D : MonoBehaviour, IUnitViewResolver
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
        _gameVM.UnitMovedByRoute += OnUnitMovedByRoute;
        _gameVM.UnitAttacked += OnUnitAttacked;
        _gameVM.UnitHit += OnUnitHit;
        _gameVM.UnitDied += OnUnitDied;
        _gameVM.UnitTurnStarted += OnUnitTurnStarted;
        _gameVM.UnitHealthChanged += OnUnitHealthChanged;
    }

    private void OnUnitMovedByRoute(IViewModel viewModel, List<Vector3> route)
    {
        var view = _views[viewModel];
        view.RequestMove(route);
        Debug.Log("OnUnitMovedByRoute");
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

    // public void OnUnitRemoved(IViewModel viewModel)
    // {
    //     if (_views.TryGetValue(viewModel, out var view))
    //     {
    //         _views.Remove(viewModel);
    //         if (view != null)
    //         {
    //             Destroy(view.gameObject);
    //         }
    //     }
    // }

    public bool TryGetView(UnitModel model, out UnitView3D view)
    {
        return _models.TryGetValue(model, out view);
    }

    // Placeholder handlers to satisfy subscriptions; real implementations likely exist elsewhere
    private void OnUnitAttacked(IViewModel viewModel, DamageContext ctx) { }
    private void OnUnitHit(IViewModel viewModel, DamageContext ctx) { }
    private void OnUnitDied(IViewModel viewModel)
    {
        if (_views.TryGetValue(viewModel, out var view))
        {
            _views.Remove(viewModel);
            if (view != null)
            {
                view.HandleDeath();
            }
        }
    }
        private void OnUnitTurnStarted(IViewModel viewModel) { }
    private void OnUnitHealthChanged(IViewModel viewModel) { }
}