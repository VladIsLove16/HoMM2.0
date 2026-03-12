using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



public class CellInputHandler : MonoBehaviour
{
    public enum InputInteractionType
    {
        BeginDrag,
        DragUpdate,
        EndDrag
    }
    public enum SelectionMode
    {
        BeginDrag,
        DragUpdate,
        EndDrag,
        GridOnly
    }
    [SerializeField] private LayerMask mouseColliderLayerMask;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameView3D _gameView3D;
    [SerializeField] private Vector2 initialPointerNormalized = new Vector2(0.5f, 0.5f);

    private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>(8);
    private Collider _lastHoveredCollider;
    private Vector2Int? _lastHoveredCell;
    private Vector2 _currentPointerPosition;
    private bool _pointerInitialized;
    private Vector3? _lastDragWorldPos;

    public bool IsDragging { get; private set; }
    public Action ActionCanceled;

    private Camera Cam => mainCamera != null ? mainCamera : Camera.main;


    private void Awake()
    {
        var width = Mathf.Max(1, Screen.width);
        var height = Mathf.Max(1, Screen.height);
        _currentPointerPosition = new Vector2(initialPointerNormalized.x * width, initialPointerNormalized.y * height);
        _pointerInitialized = false;
    }

    public void UpdatePointerPosition(Vector2 screenPosition)
    {
        _pointerInitialized = true;
        _currentPointerPosition = screenPosition;
    }

    public void HandleMouseMoved()
    {
        if (!TryGetHit(out var hit))
        {
            _gameView3D?.ClearHover();
            _lastHoveredCollider = null;
            _lastHoveredCell = null;
            return;
        }

        var obj = hit.collider.GetComponent<IGameViewObject>();
        if (obj != null && obj.IsHoverable)
        {
            if (_lastHoveredCollider == hit.collider)
                return;

            _gameView3D?.HandleGameViewObjectHovered(obj);
            _lastHoveredCollider = hit.collider;
            _lastHoveredCell = null;
            return;
        }

        if (_gameView3D != null && _gameView3D.HandleGridHover(hit.point, out var cell))
        {
            if (_lastHoveredCollider == hit.collider && _lastHoveredCell.HasValue && _lastHoveredCell.Value == cell)
                return;

            _lastHoveredCollider = hit.collider;
            _lastHoveredCell = cell;
        }
    }

    public void HandleSelect()
    {
        if (IsPointerOverUI()) return;
        if (!TryGetHit(out var hit)) return;
        var obj = hit.collider.GetComponent<IGameViewObject>();
        if (obj != null && obj.IsSelectable)
        {
            _gameView3D?.HandleGameViewObjectSelected(obj);
        }
        else
        {
            _gameView3D?.HandleGridSelect(hit.point);
        }
    }

    public void HandleAction()
    {
        if (IsPointerOverUI()) return;
        if (!TryGetHit(out var hit))
        {
            ActionCanceled?.Invoke();
            return;
        }

        var obj = hit.collider.GetComponent<IGameViewObject>();
        if (obj != null)
        {
            _gameView3D?.HandleActionPerformed(obj);
            return;
        }

        if (!(_gameView3D?.HandleGridAction(hit.point) ?? false))
        {
            ActionCanceled?.Invoke();
        }
    }

    public void HandleDragStart()
    {
        if (IsPointerOverUI()) return;
        if (!TryGetHit(out var hit)) return;
        var obj = hit.collider.GetComponent<IGameViewObject>();
        var unitView = ResolveUnitView(obj);
        if (unitView != null)
        {
            IsDragging = true;
            _lastDragWorldPos = hit.point;
            _gameView3D?.BeginDrag(unitView);
        }
    }

    public void HandleDragUpdate()
    {
        if (!IsDragging) return;
        if (!TryGetHit(out var hit))
        {
            if (_lastDragWorldPos.HasValue)
            {
                _gameView3D?.UpdateDrag(_lastDragWorldPos.Value);
            }
            return;
        }
        var obj = hit.collider.GetComponent<IGameViewObject>();
        if (!TryResolveWorldPosition(obj, out var worldPos))
        {
            worldPos = hit.point;
        }
        _lastDragWorldPos = worldPos;
        _gameView3D?.UpdateDrag(worldPos);
    }

    public void HandleDragCancel()
    {
        if (!IsDragging) return;
        IsDragging = false;
        if (!TryGetHit(out var hit))
        {
            if (_lastDragWorldPos.HasValue)
            {
                _gameView3D?.EndDrag(_lastDragWorldPos.Value);
                _lastDragWorldPos = null;
                return;
            }
            ActionCanceled?.Invoke();
            return;
        }
        var obj = hit.collider.GetComponent<IGameViewObject>();
        Vector3 worldPos;
        if (TryResolveWorldPosition(obj, out worldPos))
        {
            _lastDragWorldPos = worldPos;
            _gameView3D?.EndDrag(worldPos);
        }
        else
        {
            worldPos = hit.point;
            _lastDragWorldPos = worldPos;
            _gameView3D?.EndDrag(worldPos);
        }
        _lastDragWorldPos = null;
    }

    private UnitView3D ResolveUnitView(IGameViewObject obj)
    {
        if (obj is UnitView3D uv) return uv;
        if (obj is Component c) return c.GetComponentInParent<UnitView3D>();
        return null;
    }

    private bool TryResolveWorldPosition(IGameViewObject obj, out Vector3 worldPosition)
    {
        if (obj is Component c)
        {
            worldPosition = c.transform.position;
            return true;
        }
        worldPosition = default;
        return false;
    }

    private bool TryGetHit(out RaycastHit hit)
    {
        var cam = Cam;
        if (cam == null) { hit = default; return false; }
        var ray = cam.ScreenPointToRay(GetPointerPosition());
        Physics.Raycast(ray, out hit, 999f, mouseColliderLayerMask);
        return hit.collider != null;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = GetPointerPosition() };
        _uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _uiRaycastResults);
        return _uiRaycastResults.Count > 0;
    }

    private Vector2 GetPointerPosition()
    {
        if (_pointerInitialized)
            return _currentPointerPosition;

        return (Vector2)Input.mousePosition;
    }

#if UNITY_INCLUDE_TESTS
    public void TestSetGameView(GameView3D view) => _gameView3D = view;

    public void TestProcessGameViewObject(UnitView3D unit, object beginDrag)
    {
        throw new NotImplementedException();
    }
#endif
}
