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
    
    [Inject]
    public void Construct(GameViewModel gameVM, UnitViewFactory unitViewFactory)
    {
        Debug.Log("GameView3D inject");

        _gameVM = gameVM;
        _factory = unitViewFactory;

        _gameVM.UnitSpawned += OnUnitSpawned;
        _gameVM.UnitMovedByRoute += OnUnitMovedByRoute;
        _gameVM.UnitAttacked += OnUnitAttacked;
        _gameVM.UnitHit += OnUnitHit;
        _gameVM.UnitDied += OnUnitDied;
        _gameVM.ActionPreviewChanged += OnActionPreviewChanged;
        _gameVM.CellHovered += OnCellHovered;
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

    // MVVM input handling methods - работа только с коллайдерами и координатами
    public void HandleGameViewObjectHovered(IGameViewObject gameViewObject)
    {
        Debug.Log(gameViewObject.transform.gameObject.name + " HandleGameViewObjectHovered");
        // Подсвечиваем объект при наведении, если нужно
        if (gameViewObject is IHoverable hoverable)
        {
            hoverable.Hover();
        }
        
        // Передаем координаты в ViewModel для обработки
        _gameVM?.OnGameViewObjectHovered(gameViewObject);
    }

    public void HandleGameViewObjectSelected(IGameViewObject gameViewObject)
    {
        // Передаем координаты в ViewModel для обработки
        _gameVM?.OnGameViewObjectSelected(gameViewObject);
    }

    public void HandleActionPerformed(IGameViewObject gameViewObject)
    {
        // Передаем координаты в ViewModel для обработки
        _gameVM?.OnActionPerformed(gameViewObject);
    }

    private void OnActionPreviewChanged(ActionPreview preview)
    {
        // View only handles visual representation, not overlay logic
        // Overlay logic is now handled by GameViewModel
    }

    private void OnCellHovered(Vector2Int cellCoords)
    {
        // Handle cell hover logic if needed
        // This could trigger additional visual feedback
    }


    private void OnDestroy()
    {
        if (_gameVM != null)
        {
            _gameVM.UnitSpawned -= OnUnitSpawned;
            _gameVM.UnitMovedByRoute -= OnUnitMovedByRoute;
            _gameVM.UnitAttacked -= OnUnitAttacked;
            _gameVM.UnitHit -= OnUnitHit;
            _gameVM.UnitDied -= OnUnitDied;
            _gameVM.ActionPreviewChanged -= OnActionPreviewChanged;
            _gameVM.CellHovered -= OnCellHovered;
        }
    }
}