// GameView3D.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameView3D : MonoBehaviour
{
    [SerializeField] bool Log;
    private UnitViewFactory _factory;
    private Dictionary<IViewModel, UnitView3D> _views = new();
    private GameViewModel _gameVM;
    [Inject]private IWorldToCellProvider _worldToCellProvider;
    private UnitView3D _draggedUnit;

    public Action<KeyValuePair<Vector2Int, Vector2Int>> TestHandleCellHovered;
    public Action<KeyValuePair<Vector2Int, Vector2Int>> TestHandleCellSelected;

    public void SetWorldToCellProvider(IWorldToCellProvider provider) => _worldToCellProvider = provider;

    [Inject]
    public void Construct(GameViewModel gameVM, UnitViewFactory unitViewFactory)
    {
        Debug.Log("GameView3D inject");

        _gameVM = gameVM;
        _factory = unitViewFactory;

        gameVM.UnitSpawned += OnGameVM_UnitSpawned;
    }
    private void OnGameVM_UnitSpawned(UnitViewModel model)
    {
       var view =  _factory.Create(model);
        _views[model] = view;
    }

    public void HandleGameViewObjectHovered(IGameViewObject gameViewObject)
    {
        if(Log)
            Debug.Log(gameViewObject.transform.gameObject.name + " HandleGameViewObjectHovered");
        if (gameViewObject is IHoverable hoverable)
        {
            hoverable.Hover();
        }
        _worldToCellProvider.ToGridPair(gameViewObject.transform.position, out var coords);
        TestHandleCellHovered?.Invoke(coords);
        _gameVM?.HandleCellHovered(coords);
    }

    public void HandleGameViewObjectSelected(IGameViewObject gameViewObject)
    {
        _worldToCellProvider.ToGridPair(gameViewObject.transform.position, out var coords);
        TestHandleCellSelected?.Invoke(coords);
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
        if(Log)
        Debug.Log(_draggedUnit.gameObject.name);
    }

    public virtual void UpdateDrag(Vector3 worldPos)
    {
        if (Log)
            Debug.Log("update drag");
        if (_draggedUnit != null)
            _draggedUnit.transform.position = worldPos + Vector3.up * 0.1f;
    }

    public void EndDrag(Vector3 worldPos)
    {
        if (_draggedUnit == null)
        {
            return;
        }

        var resolvedWorldPos = worldPos;

        if (_worldToCellProvider != null && _worldToCellProvider.ToGrid(worldPos, out var coords))
        {
            resolvedWorldPos = _worldToCellProvider.ToWorld(coords.x, coords.y);

            if (_gameVM != null)
            {
                foreach (var entry in _views)
                {
                    if (entry.Value == _draggedUnit)
                    {
                        _gameVM.SnapToCell(entry.Key as UnitViewModel, coords);
                        entry.Value.SnapToCell(resolvedWorldPos);
                        _draggedUnit = null;
                        return;
                    }
                }
            }
        }

        _draggedUnit.transform.position = resolvedWorldPos;
        _draggedUnit = null;
    }

    private void OnDestroy()
    {
        if (_gameVM != null)
        {
        }
    }

   
}

