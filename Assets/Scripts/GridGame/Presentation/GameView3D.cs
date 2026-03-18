// GameView3D.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

public class GameView3D : MonoBehaviour
{
    [SerializeField] bool Log;
    private UnitViewFactory _factory;
    private Dictionary<IViewModel, UnitView3D> _views = new();
    private GameViewModel _gameVM;
    [Inject] private IWorldToCellProvider _worldToCellProvider;
    [Inject(Optional = true)] private IBattleAnimationGate _animationGate;
    [Inject(Optional = true)] private ITurnStateViewModel _turnState;
    private UnitView3D _draggedUnit;
    private IHoverable _lastHoverable;
    private UnitView3D _hoveredUnitView;
    private Vector3 _dragOffset;
    private float _dragY;
    private readonly CompositeDisposable _subscriptions = new();
    private readonly Dictionary<UnitViewModel, Vector2Int> _deadUnits = new();

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
        _gameVM.HoveredUnitChanged += OnHoveredUnitChanged;

        if (_turnState != null)
        {
            _turnState.BattleStateProperty
                .Subscribe(_ => UpdateUnitVisibility())
                .AddTo(_subscriptions);
        }
    }
    private void OnGameVM_UnitSpawned(UnitViewModel model)
    {
       var view =  _factory.Create(model);
        _views[model] = view;
        UpdateUnitVisibility(view, model);

        model.OnDeath
            .Subscribe(_ => RegisterDeadUnit(model))
            .AddTo(_subscriptions);

        model.OnPosChanged
            .Subscribe(_ => RefreshDeadBodyVisibility())
            .AddTo(_subscriptions);
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

        if (_lastHoverable != null && !ReferenceEquals(_lastHoverable, gameViewObject))
        {
            _lastHoverable.Unhover();
            _lastHoverable = null;
        }

        if (Log)
            Debug.Log(gameViewObject.transform.gameObject.name + " HandleGameViewObjectHovered");

        if (gameViewObject is IHoverable hoverable && gameViewObject is not UnitView3D)
        {
            hoverable.Hover();
            _lastHoverable = hoverable;
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

        ClearHoverable();
        cell = coords.Key;
        ProcessCellHover(coords);
        return true;
    }

    public void ClearHover()
    {
        ClearHoverable();
        _gameVM?.HandleCellHovered((Vector2Int?)null);
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
    public void BeginDrag(UnitView3D unit, Vector3 grabWorldPosition)
    {
        if (IsInteractionLocked())
        {
            Debug.LogWarning("[GameView3D] BeginDrag blocked: interaction locked.");
            return;
        }
        _draggedUnit = unit;
        if (_draggedUnit != null)
        {
            var unitPosition = _draggedUnit.transform.position;
            _dragOffset = unitPosition - new Vector3(grabWorldPosition.x, unitPosition.y, grabWorldPosition.z);
            _dragOffset.y = 0f;
            _dragY = unitPosition.y;
        }
        if(Log)
        Debug.Log(_draggedUnit.gameObject.name);
    }

    public void CancelDrag()
    {
        _draggedUnit = null;
        _dragOffset = Vector3.zero;
    }

    public bool TryGetUnitAtWorld(Vector3 worldPosition, out UnitView3D unitView)
    {
        unitView = null;
        if (_worldToCellProvider == null || _gameVM == null)
            return false;

        if (!_worldToCellProvider.ToGrid(worldPosition, out var cell))
            return false;

        if (!_gameVM.IsInBounds(cell))
            return false;

        if (!_gameVM.TryGetUnitAtCell(cell, out var vm))
            return false;

        return _views.TryGetValue(vm, out unitView) && unitView != null;
    }

    public virtual void UpdateDrag(Vector3 worldPos)
    {
        if (Log)
            Debug.Log("update drag");
        if (_draggedUnit == null)
            return;

        var targetPosition = worldPos + _dragOffset;
        targetPosition.y = _dragY;
        _draggedUnit.transform.position = targetPosition;
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
            if (_gameVM != null && !_gameVM.IsInBounds(coords))
            {
                if (_gameVM.TryGetUnitCell(ResolveDraggedVM(), out var currentCell))
                {
                    resolvedWorldPos = _worldToCellProvider.ToWorld(currentCell.x, currentCell.y);
                    _draggedUnit.SnapToCell(resolvedWorldPos);
                    _draggedUnit = null;
                    _dragOffset = Vector3.zero;
                    return;
                }
            }

            resolvedWorldPos = _worldToCellProvider.ToWorld(coords.x, coords.y);

            if (_gameVM != null)
            {
                foreach (var entry in _views)
                {
                    if (entry.Value == _draggedUnit)
                    {
                        var draggedVm = entry.Key as UnitViewModel;
                        if (_gameVM.TrySetCell(draggedVm, coords))
                        {
                            entry.Value.SnapToCell(resolvedWorldPos);
                        }
                        else if (_gameVM.TryGetUnitCell(draggedVm, out var currentCell))
                        {
                            var backWorld = _worldToCellProvider.ToWorld(currentCell.x, currentCell.y);
                            entry.Value.SnapToCell(backWorld);
                        }
                        _draggedUnit = null;
                        _dragOffset = Vector3.zero;
                        return;
                    }
                }
            }
        }

        resolvedWorldPos.y = _dragY;
        _draggedUnit.transform.position = resolvedWorldPos;
        _draggedUnit = null;
        _dragOffset = Vector3.zero;
    }

    private UnitViewModel ResolveDraggedVM()
    {
        if (_draggedUnit == null)
            return null;

        foreach (var entry in _views)
        {
            if (entry.Value == _draggedUnit)
            {
                return entry.Key as UnitViewModel;
            }
        }

        return null;
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

    private void ClearHoverable()
    {
        if (_lastHoverable == null)
            return;

        _lastHoverable.Unhover();
        _lastHoverable = null;
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
            _gameVM.HoveredUnitChanged -= OnHoveredUnitChanged;
        }
        _subscriptions.Dispose();
    }

    private bool IsInteractionLocked() => _animationGate != null && _animationGate.IsLocked;

    private void UpdateUnitVisibility()
    {
        if (_turnState == null)
            return;

        foreach (var kvp in _views)
        {
            UpdateUnitVisibility(kvp.Value, kvp.Key as UnitViewModel);
        }
    }

    private void UpdateUnitVisibility(UnitView3D view, UnitViewModel vm)
    {
        if (view == null || vm == null || _turnState == null)
            return;

        var hideEnemies = _turnState.BattleStateProperty.Value == BattleState.replacement;
        var shouldShow = !hideEnemies || vm.Team == _turnState.LocalTeam;
        if (view.gameObject.activeSelf != shouldShow)
        {
            view.gameObject.SetActive(shouldShow);
        }
    }

    private void RegisterDeadUnit(UnitViewModel vm)
    {
        if (vm == null)
            return;

        _deadUnits[vm] = vm.Model.Position.Value;
        RefreshDeadBodyVisibility();
    }

    private void RefreshDeadBodyVisibility()
    {
        if (_gameVM == null || _deadUnits.Count == 0)
            return;

        foreach (var kvp in _deadUnits)
        {
            if (!_views.TryGetValue(kvp.Key, out var view) || view == null)
                continue;

            var cell = kvp.Value;
            var occupied = _gameVM.IsCellOccupied(cell);
            view.SetCorpseVisible(!occupied);
        }
    }

    private void OnHoveredUnitChanged(UnitViewModel hoveredUnit)
    {
        UnitView3D next = null;
        if (hoveredUnit != null)
        {
            _views.TryGetValue(hoveredUnit, out next);
        }

        if (_hoveredUnitView == next)
            return;

        _hoveredUnitView?.Unhover();
        _hoveredUnitView = next;
        _hoveredUnitView?.Hover();
    }
}
