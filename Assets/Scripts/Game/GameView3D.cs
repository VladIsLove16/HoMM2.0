// GameView3D.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameView3D : MonoBehaviour
{
    private UnitViewFactory _factory;
    private Dictionary<UnitViewModel, UnitView3D> _views = new();
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
    }
    public virtual void OnUnitSpawned(UnitViewModel viewModel)
    {
        var view = _factory.Create(viewModel);
        _views[viewModel] = view;
        UnitModel unitModel = viewModel.Model;
        _models[unitModel] = view;
    }

    public void OnUnitRemoved(UnitViewModel viewModel)
    {
        if (_views.TryGetValue(viewModel, out var view))
        {
            GameObject.Destroy(view.gameObject);
            _views.Remove(viewModel);

        }
    }
    public UnitView3D GetView(UnitModel model)
    {
        return _models[model];
    }
}