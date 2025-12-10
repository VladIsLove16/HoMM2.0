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
    [Inject] private IWorldToCellProvider _worldToCellProvider;
    [Inject(Optional = true)] private IBattleAnimationGate _animationGate;
    private UnitView3D _draggedUnit;

    public Action<KeyValuePair<Vector2Int, Vector2Int>> TestHandleCellHovered;
    public Action<KeyValuePair<Vector2Int, Vector2Int>> TestHandleCellSelected;

    public void SetWorldToCellProvider(IWorldToCellProvider provider)
    {
        _worldToCellProvider = provider;
    }

    [Inject]
    public void Construct(
        [InjectOptional] GameViewModel gameVM,
        [InjectOptional] UnitViewFactory unitViewFactory)
    {
        _gameVM = gameVM;
        _factory = unitViewFactory;

        if (_gameVM == null || _factory == null)
        {
            Debug.LogError($"[GameView3D] Missing dependencies. GameViewModel: {_gameVM != null}, UnitViewFactory: {_factory != null}. Component disabled.", this);
            enabled = false;
            return;
        }

        _gameVM.UnitSpawned += OnGameVM_UnitSpawned;
        _gameVM.AttackAnimationRequested += OnAttackAnimationRequested;
    }
    private void OnGameVM_UnitSpawned(UnitViewModel model)
    {
       var view =  _factory.Create(model);
        _views[model] = view;
    }

    private void OnAttackAnimationRequested(UnitViewModel attackerVm, UnitViewModel defenderVm)
    {
        if (attackerVm == null || defenderVm == null)
            return;

        if (!_views.TryGetValue(attackerVm, out var attackerView))
            return;

        if (!_views.TryGetValue(defenderVm, out var defenderView))
            return;

        attackerView?.PlayCoordinatedAttack(defenderView);
        defenderView?.PlayCoordinatedHit(attackerView);
    }

    public void HandleGameViewObjectHovered(IGameViewObject gameViewObject)
    {
        if (gameViewObject == null)
            return;

        if (Log)
            Debug.Log(gameViewObject.transform.gameObject.name + " HandleGameViewObjectHovered");

        if (gameViewObject is IHoverable hoverable)
        {
            hoverable.Hover();
        }

        if (TryResolveCoords(gameViewObject.transform.position, out var coords))
        {
            ProcessCellHover(coords);
        }
    }

    public void HandleGameViewObjectSelected(IGameViewObject gameViewObject)
    {
        if (gameViewObject == null || IsInteractionLocked())
            return;

        if (TryResolveCoords(gameViewObject.transform.position, out var coords))
        {
            ProcessCellSelected(coords);
        }
    }

    public void HandleActionPerformed(IGameViewObject gameViewObject)
    {
        if (gameViewObject == null)
            return;

        if (TryResolveCoords(gameViewObject.transform.position, out var coords))
        {
            ProcessCellAction(coords);
        }
    }

    public bool HandleGridHover(Vector3 worldPosition, out Vector2Int cell)
    {
        cell = default;
        if (!TryResolveCoords(worldPosition, out var coords))
            return false;

        cell = coords.Key;
        ProcessCellHover(coords);
        return true;
    }

    public bool HandleGridSelect(Vector3 worldPosition)
    {
        if (IsInteractionLocked())
            return false;

        if (!TryResolveCoords(worldPosition, out var coords))
            return false;

        ProcessCellSelected(coords);
        return true;
    }

    public bool HandleGridAction(Vector3 worldPosition)
    {
        if (!TryResolveCoords(worldPosition, out var coords))
            return false;

        ProcessCellAction(coords);
        return true;
    }
    public void SnapUnitToCell(UnitView3D unitView, Vector2Int cell)
    {
        var worldPos = _worldToCellProvider.ToWorld(cell.x,cell.y);
        unitView.SnapToCell(worldPos);
    }
    public void BeginDrag(UnitView3D unit)
    {
        if (IsInteractionLocked())
            return;
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
        if (IsInteractionLocked())
            return;
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
                        _gameVM.SetCell(entry.Key as UnitViewModel, coords);
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

    private bool TryResolveCoords(Vector3 worldPosition, out KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        coords = default;
        if (_worldToCellProvider == null)
            return false;

        return _worldToCellProvider.ToGridPair(worldPosition, out coords);
    }

    private void ProcessCellHover(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        TestHandleCellHovered?.Invoke(coords);
        _gameVM?.HandleCellHovered(coords.Key, coords.Value);
    }

    private void ProcessCellSelected(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        TestHandleCellSelected?.Invoke(coords);
        _gameVM?.HandleCellSelected(coords);
    }

    private void ProcessCellAction(KeyValuePair<Vector2Int, Vector2Int> coords)
    {
        _gameVM?.HandleCellActionPerformed(coords.Key, coords.Value);
    }

    private void OnDestroy()
    {
        if (_gameVM != null)
        {
            _gameVM.AttackAnimationRequested -= OnAttackAnimationRequested;
            _gameVM.UnitSpawned -= OnGameVM_UnitSpawned;
        }
    }

    private bool IsInteractionLocked() => _animationGate != null && _animationGate.IsLocked;
}
