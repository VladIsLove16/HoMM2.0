using System;
using System.Collections.Generic;
using UnityEngine;

public class CellView : MonoBehaviour, IGameViewObject
{
    [SerializeField] private MeshRenderer _hoverRenderer;
    [SerializeField] private MeshRenderer _routePointRenderer;

    private Renderer _baseRenderer;
    private CellMaterialRegistry _materials = CellMaterialRegistry.Empty;
    private readonly CellStateCollection _states = new();
    private CellAppearanceController _appearance;

    public bool IsHoverable => true;
    public bool IsSelectable => true;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        _appearance.Apply(_states);
    }

    private void OnDestroy()
    {
        _states.Changed -= OnStatesChanged;
    }

    internal void Init(IReadOnlyDictionary<CellState, CellMaterial> materials)
    {
        EnsureInitialized();
        _materials = new CellMaterialRegistry(materials);
        _appearance.Apply(_states);
    }

    public void SetHoverRenderer(MeshRenderer renderer)
    {
        _hoverRenderer = renderer;
        _appearance?.Apply(_states);
    }

    public void SetRoutePointRenderer(MeshRenderer renderer)
    {
        _routePointRenderer = renderer;
        _appearance?.Apply(_states);
    }

    public void SetMaterialsDictionary(IReadOnlyDictionary<CellState, CellMaterial> materials)
    {
        EnsureInitialized();
        _materials = new CellMaterialRegistry(materials);
        _appearance.Apply(_states);
    }

    public void AddState(CellState state)
    {
        if (_states.Add(state))
        {
            _appearance.Apply(_states);
        }
    }

    public void RemoveState(CellState state)
    {
        if (_states.Remove(state))
        {
            _appearance.Apply(_states);
        }
    }

    public CellState[] GetStates()
    {
        return _states.ToArray();
    }

    private void OnStatesChanged()
    {
        _appearance.Apply(_states);
    }

    private void EnsureInitialized()
    {
        if (_appearance != null)
            return;

        _baseRenderer = GetComponent<Renderer>();
        _appearance = new CellAppearanceController(
            () => _baseRenderer,
            () => _hoverRenderer,
            () => _routePointRenderer,
            () => _materials);

        _states.Changed += OnStatesChanged;
    }

    private sealed class CellStateCollection
    {
        private static readonly Dictionary<CellState, int> Priorities = new()
        {
            { CellState.activeUnit, 500 },
            { CellState.attackTargetBlocked, 460 },
            { CellState.attackTarget, 450 },
            { CellState.enemyCell, 320 },
            { CellState.hoveredEnemy, 420 },
            { CellState.reachableCell, 400 },
            { CellState.enemyReachableCell, 410 },
            { CellState.hovered, 250 },
            { CellState.routeEndAccessible, 220 },
            { CellState.routeEndBlocked, 210 },
            { CellState.accessibleRoutePoint, 200 },
            { CellState.inaccessibleRoutePoint, 150 },
            { CellState.normal, 0 }
        };

        private readonly List<CellState> _states = new();
        public event Action Changed;

        public bool Add(CellState state)
        {
            if (_states.Contains(state))
                return false;

            _states.Add(state);
            _states.Sort((a, b) => GetPriority(b).CompareTo(GetPriority(a)));
            Changed?.Invoke();
            return true;
        }

        public bool Remove(CellState state)
        {
            if (_states.Remove(state))
            {
                Changed?.Invoke();
                return true;
            }

            return false;
        }

        public bool Contains(CellState state) => _states.Contains(state);

        public CellState[] ToArray() => _states.ToArray();

        public IReadOnlyList<CellState> States => _states;

        private static int GetPriority(CellState state)
        {
            return Priorities.TryGetValue(state, out var priority) ? priority : -1;
        }
    }

    private sealed class CellMaterialRegistry
    {
        private readonly IReadOnlyDictionary<CellState, CellMaterial> _source;

        public static CellMaterialRegistry Empty { get; } = new CellMaterialRegistry(null);

        public CellMaterialRegistry(IReadOnlyDictionary<CellState, CellMaterial> source)
        {
            _source = source ?? new Dictionary<CellState, CellMaterial>();
        }

        public bool TryGet(CellState state, out Material material)
        {
            if (_source != null && _source.TryGetValue(state, out var cellMaterial) && cellMaterial != null)
            {
                material = cellMaterial.Material;
                return material != null;
            }

            material = null;
            return false;
        }
    }

    private sealed class CellAppearanceController
    {
        private readonly Func<Renderer> _baseRendererProvider;
        private readonly Func<MeshRenderer> _hoverRendererProvider;
        private readonly Func<MeshRenderer> _routeRendererProvider;
        private readonly Func<CellMaterialRegistry> _materialsProvider;

        public CellAppearanceController(
            Func<Renderer> baseRendererProvider,
            Func<MeshRenderer> hoverRendererProvider,
            Func<MeshRenderer> routeRendererProvider,
            Func<CellMaterialRegistry> materialsProvider)
        {
            _baseRendererProvider = baseRendererProvider;
            _hoverRendererProvider = hoverRendererProvider;
            _routeRendererProvider = routeRendererProvider;
            _materialsProvider = materialsProvider;
        }

        public void Apply(CellStateCollection states)
        {
            ApplyHover(states);
            ApplyRoute(states);
            ApplyBase(states);
        }

        private void ApplyHover(CellStateCollection states)
        {
            var hover = _hoverRendererProvider?.Invoke();
            if (hover == null)
                return;

            hover.gameObject.SetActive(states.Contains(CellState.hovered));
        }

        private void ApplyRoute(CellStateCollection states)
        {
            var route = _routeRendererProvider?.Invoke();
            if (route == null)
                return;

            bool show = states.Contains(CellState.accessibleRoutePoint)
                        || states.Contains(CellState.inaccessibleRoutePoint)
                        || states.Contains(CellState.routeEndAccessible)
                        || states.Contains(CellState.routeEndBlocked)
                        || states.Contains(CellState.enemyCell);
            route.gameObject.SetActive(show);

            if (!show)
                return;

            var materials = _materialsProvider();
            if (states.Contains(CellState.routeEndAccessible) && materials.TryGet(CellState.routeEndAccessible, out var endAccessible))
            {
                route.material = endAccessible;
            }
            else if (states.Contains(CellState.routeEndBlocked) && materials.TryGet(CellState.routeEndBlocked, out var endBlocked))
            {
                route.material = endBlocked;
            }
            else if (states.Contains(CellState.enemyCell) && materials.TryGet(CellState.enemyCell, out var enemyCell))
            {
                route.material = enemyCell;
            }
            else if (states.Contains(CellState.accessibleRoutePoint) && materials.TryGet(CellState.accessibleRoutePoint, out var accessible))
            {
                route.material = accessible;
            }
            else if (states.Contains(CellState.inaccessibleRoutePoint) && materials.TryGet(CellState.inaccessibleRoutePoint, out var blocked))
            {
                route.material = blocked;
            }
        }

        private void ApplyBase(CellStateCollection states)
        {
            var renderer = _baseRendererProvider?.Invoke();
            if (renderer == null)
                return;

            var materials = _materialsProvider();
            if (TryApply(renderer, materials, states, CellState.activeUnit)) return;
            if (TryApply(renderer, materials, states, CellState.attackTargetBlocked)) return;
            if (TryApply(renderer, materials, states, CellState.attackTarget)) return;
            if (TryApply(renderer, materials, states, CellState.hoveredEnemy)) return;
            if (TryApply(renderer, materials, states, CellState.enemyReachableCell)) return;
            if (TryApply(renderer, materials, states, CellState.reachableCell)) return;
            if (materials.TryGet(CellState.normal, out var normal))
            {
                renderer.material = normal;
            }
        }

        private static bool TryApply(Renderer renderer, CellMaterialRegistry materials, CellStateCollection states, CellState state)
        {
            if (states.Contains(state) && materials.TryGet(state, out var material))
            {
                renderer.material = material;
                return true;
            }

            return false;
        }
    }
}
