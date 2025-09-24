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
    private IWorldToCellProvider _worldToCellProvider;
    
    [Inject]
    public void Construct(GameViewModel gameVM, UnitViewFactory unitViewFactory)
    {
        Debug.Log("GameView3D inject");

        _gameVM = gameVM;
        _factory = unitViewFactory;

    }

    public void HandleGameViewObjectHovered(IGameViewObject gameViewObject)
    {
        Debug.Log(gameViewObject.transform.gameObject.name + " HandleGameViewObjectHovered");
        // Подсвечиваем объект при наведении, если нужно
        if (gameViewObject is IHoverable hoverable)
        {
            hoverable.Hover();
        }
        _worldToCellProvider.ToGridPair(gameViewObject.transform.position, out var coords);
        _gameVM?.HandleCellHovered(coords);
    }

    public void HandleGameViewObjectSelected(IGameViewObject gameViewObject)
    {
        _worldToCellProvider.ToGridPair(gameViewObject.transform.position, out var coords);
        _gameVM?.HandleCellSelected(coords);
    }

    public void HandleActionPerformed(IGameViewObject gameViewObject)
    {
        _worldToCellProvider.ToGridPair(gameViewObject.transform.position, out var coords);
        _gameVM?.HandleCellActionPerformed(coords);
    }
    public bool TryGetView(UnitModel model, out UnitView3D view)
    {
        if(_models.TryGetValue(model, out view))
        return true;
        return false;
    }

    private void OnDestroy()
    {
        if (_gameVM != null)
        {
        }
    }

   
}