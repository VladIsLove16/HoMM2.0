using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;



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
    private Plane _dragPlane;
    private bool _dragPlaneReady;

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

            _gameView3D?.HandleGameViewObjectHovered(obj, hit.point);
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
            _gameView3D?.HandleGameViewObjectSelected(obj, hit.point);
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
            _gameView3D?.HandleActionPerformed(obj, hit.point);
            return;
        }

        if (!(_gameView3D?.HandleGridAction(hit.point) ?? false))
        {
            ActionCanceled?.Invoke();
        }
    }

    public void HandleDragStart()
    {
        if (IsPointerOverUI())
        {
            Debug.Log("[CellInputHandler] DragStart blocked: pointer over UI.");
            return;
        }
        if (!TryGetHit(out var hit))
        {
            Debug.Log("[CellInputHandler] DragStart blocked: no hit under cursor.");
            return;
        }
        var obj = hit.collider.GetComponent<IGameViewObject>();
        var unitView = ResolveUnitView(obj);
        if (unitView == null)
        {
            unitView = hit.collider.GetComponentInParent<UnitView3D>();
        }
        if (unitView == null && _gameView3D != null && _gameView3D.TryGetUnitAtWorld(hit.point, out var viewFromCell))
        {
            unitView = viewFromCell;
        }
        if (unitView != null)
        {
            IsDragging = true;
            _lastDragWorldPos = hit.point;
            _dragPlane = new Plane(Vector3.up, hit.point);
            _dragPlaneReady = true;
            _gameView3D?.BeginDrag(unitView, hit.point);
        }
        else
        {
            Debug.Log("[CellInputHandler] DragStart blocked: no UnitView3D under cursor.");
        }
    }

    public void HandleDragUpdate()
    {
        if (!IsDragging) return;
        var worldPos = TryGetDragPlanePoint(out var planePoint)
            ? planePoint
            : (_lastDragWorldPos ?? Vector3.zero);
        _lastDragWorldPos = worldPos;
        _gameView3D?.UpdateDrag(worldPos);
    }

    public void HandleDragCancel()
    {
        if (!IsDragging) return;
        IsDragging = false;
        _dragPlaneReady = false;
        if (TryGetDragPlanePoint(out var planePoint))
        {
            _lastDragWorldPos = planePoint;
            _gameView3D?.EndDrag(planePoint);
            _lastDragWorldPos = null;
            return;
        }

        if (_lastDragWorldPos.HasValue)
        {
            _gameView3D?.EndDrag(_lastDragWorldPos.Value);
            _lastDragWorldPos = null;
            return;
        }

        if (TryGetHit(out var hit))
        {
            var worldPos = hit.point;
            _lastDragWorldPos = worldPos;
            _gameView3D?.EndDrag(worldPos);
            _lastDragWorldPos = null;
            return;
        }

        ActionCanceled?.Invoke();
    }

    public void ClearInteractionState()
    {
        IsDragging = false;
        _dragPlaneReady = false;
        _lastDragWorldPos = null;
        _lastHoveredCollider = null;
        _lastHoveredCell = null;
        _gameView3D?.CancelDrag();
        _gameView3D?.ClearHover();
    }

    private UnitView3D ResolveUnitView(IGameViewObject obj)
    {
        if (obj is UnitView3D uv) return uv;
        if (obj is Component c) return c.GetComponentInParent<UnitView3D>();
        return null;
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

        var pointer = Pointer.current;
        if (pointer != null)
        {
            _currentPointerPosition = pointer.position.ReadValue();
            _pointerInitialized = true;
        }

        return _currentPointerPosition;
    }

    private bool TryGetDragPlanePoint(out Vector3 worldPos)
    {
        worldPos = default;
        if (!_dragPlaneReady)
            return false;

        var cam = Cam;
        if (cam == null)
            return false;

        var ray = cam.ScreenPointToRay(GetPointerPosition());
        if (_dragPlane.Raycast(ray, out var enter))
        {
            worldPos = ray.GetPoint(enter);
            return true;
        }

        return false;
    }

#if UNITY_INCLUDE_TESTS
    public void TestSetGameView(GameView3D view) => _gameView3D = view;

    public void TestProcessGameViewObject(UnitView3D unit, object beginDrag)
    {
        throw new NotImplementedException();
    }
#endif
}
