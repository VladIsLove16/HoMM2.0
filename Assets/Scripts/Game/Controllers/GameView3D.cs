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

public class GameView3D : MonoBehaviour
{
    private UnitViewFactory _factory;
    private Dictionary<IViewModel, UnitView3D> _views = new();
    private GameViewModel _gameVM;
    private IWorldToCellProvider _worldToCellProvider;
    private UnitView3D _draggedUnit;

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
    public void SnapUnitToCell(UnitView3D unitView, Vector2Int cell)
    {
        var worldPos = _worldToCellProvider.ToWorld(cell.x,cell.y);
        unitView.SnapToCell(worldPos);
    }
    public void BeginDrag(UnitView3D unit)
    {
        _draggedUnit = unit;
    }

    public void UpdateDrag(Vector3 worldPos)
    {
        if (_draggedUnit != null)
            _draggedUnit.transform.position = worldPos + Vector3.up * 0.1f;
    }

    public void EndDrag(Vector3 worldPos)
    {
        if (_draggedUnit == null) return;

        _worldToCellProvider.ToGrid(worldPos, out var coords);
        var worldCellPos = _worldToCellProvider.ToWorld(coords.x, coords.y);
        var vm =  _views.First(IGridContent => IGridContent.Value == _draggedUnit).Key as UnitViewModel;
        _gameVM.SnapToCell(vm, coords);

        _draggedUnit = null;
    }

    private void OnDestroy()
    {
        if (_gameVM != null)
        {
        }
    }

   
}