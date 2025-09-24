using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Zenject;

public class GameViewModel : IDisposable
{
    private readonly GameModel _gameModel;
    private readonly MovementSystem _movementSystem;
    private readonly CommandService _commandService;
    [Inject] private TurnSystem _turnSystem;

    private Vector2Int? _selectedCell;

    public event Action<int, int> GridInitialized;
    public event Action<IViewModel> UnitSpawned;                        // глобальные широковещательные события
    public event Action<IViewModel, List<Vector3>> UnitMovedByRoute;    // глобальные широковещательные события
    public event Action<IViewModel, DamageContext> UnitAttacked;        // глобальные широковещательные события
    public event Action<IViewModel, DamageContext> UnitHit;             // глобальные широковещательные события
    public event Action<IViewModel> UnitDied;                           // глобальные широковещательные события
    public event Action<IViewModel> UnitTurnStarted;                    // глобальные широковещательные события
    [Obsolete("Для UI конкретного юнита используйте UnitViewModel.OnHealthChanged. Это событие предназначено для глобальных слушателей.")]
    public event Action<IViewModel> UnitHealthChanged;                  // глобальные; не использовать в Unit View/UI
    
    // Presentation layer events
    public event Action<ActionPreview> ActionPreviewChanged;           // For IAttackActionPanel, ICursorService
    public event Action<UnitModel> UnitStatsRequested;                 // For UnitStatsPanel
    public event Action<Vector2Int> CellHovered;                       // For overlay updates
    public event Action<Vector2Int> CellSelected;                     // For overlay updates
    private Dictionary<IGridContent, IViewModel> _uvms = new Dictionary<IGridContent, IViewModel>();
    private List<Vector2Int> ReachableCells = new();
    [Inject] private UnitViewModelFactory _unitViewModelsFactory;
    [Inject] IWorldToCellProvider _worldToCellProvider;
    [Inject] ActionHandlerFactory _actionHandlerFactory;

    public int ActiveUnitChanged { get; internal set; }

    public GameViewModel(GameModel model, MovementSystem movementSystem, CommandService commandService)
    {
        _gameModel = model;
        _movementSystem = movementSystem;
        _commandService = commandService;

        _gameModel.UnitSpawned += OnUnitSpawned;
        _gameModel.UnitDied += OnUnitDied;
        _gameModel.UnitMovedByRoute += OnUnitMovedByRoute; 
        _gameModel.GridInitialized += g => GridInitialized?.Invoke(g.GetWidth(), g.GetHeight());
        // Turn system subscriptions for overlay behaviors
        if (_turnSystem != null && _turnSystem.ActiveObject != null)
        {
            _turnSystem.ActiveObject.Subscribe(_ => OnActiveUnitChanged());
        }
    }

    private void OnUnitMovedByRoute(IGridContent content, List<Vector2Int> list)
    {
        var worldRoute = list.Select(coords => _worldToCellProvider.ToWorld(coords.x, coords.y)).ToList();
        var vm = _uvms[content];
        UnitMovedByRoute?.Invoke(vm, worldRoute);
    }

    private void OnUnitDied(ContentDiedParams @params)
    {
        IViewModel uvm = _uvms[@params.UnitModel];
        _uvms.Remove(@params.UnitModel);
        UnitDied?.Invoke(uvm);
    }

    protected virtual void OnUnitSpawned(UnitModelCreatedParams @params)
    {
        UnitViewModel uvm = _unitViewModelsFactory.Create(@params.UnitModel);
        _uvms[@params.UnitModel] = uvm;
        
        // Подписываемся на события модели для единообразной обработки
        @params.UnitModel.Attacked += (context) => OnUnitAttacked(@params.UnitModel, context);
        @params.UnitModel.Hitted += (context) => OnUnitHit(@params.UnitModel, context);
        @params.UnitModel.Died += () => OnUnitDied(@params.UnitModel);
        @params.UnitModel.TurnStarted += () => OnUnitTurnStarted(@params.UnitModel);
        // Здоровье ретранслируется только для глобальных слушателей.
        // Для UI юнита следует подписываться на UnitViewModel.OnHealthChanged
        @params.UnitModel.HealthChanged += () => OnUnitHealthChanged(@params.UnitModel);
        
        UnitSpawned?.Invoke(uvm);
    }
    
    private void OnUnitAttacked(UnitModel model, DamageContext context)
    {
        if (_uvms.TryGetValue(model, out var vm))
        {
            UnitAttacked?.Invoke(vm, context);
        }
    }
    
    private void OnUnitHit(UnitModel model, DamageContext context)
    {
        if (_uvms.TryGetValue(model, out var vm))
        {
            UnitHit?.Invoke(vm, context);
        }
    }
    
    private void OnUnitDied(UnitModel model)
    {
        if (_uvms.TryGetValue(model, out var vm))
        {
            UnitDied?.Invoke(vm);
        }
    }
    
    private void OnUnitTurnStarted(UnitModel model)
    {
        if (_uvms.TryGetValue(model, out var vm))
        {
            UnitTurnStarted?.Invoke(vm);
        }
    }
    
    private void OnUnitHealthChanged(UnitModel model)
    {
        if (_uvms.TryGetValue(model, out var vm))
        {
            UnitHealthChanged?.Invoke(vm);
        }
    }

    // Управление выбранной клеткой
    public void SetSelectedCell(Vector2Int cell)
    {
        _selectedCell = cell;
    }

    public Vector2Int? GetSelectedCell()
    {
        return _selectedCell;
    }


    // MVVM input handling methods - работа только с координатами
    public void OnGameViewObjectHovered(IGameViewObject gameViewObject)
    {
        _worldToCellProvider.ToGrid(gameViewObject.transform.position, out var gridCoords);
        CellHovered?.Invoke(gridCoords);
        // Determine action preview based on current context
        var preview = DetermineActionPreview(gridCoords);
        Debug.Log("gridCoords hovered " + preview.ToString());
        if (preview.HasValue)
        {
            ActionPreviewChanged?.Invoke(preview.Value);
        }
    }

    // Command execution methods - View -> ViewModel -> CommandService
    public void ExecuteMoveCommand(ulong unitId, List<Vector2Int> route)
    {
        _commandService.ExecuteMoveCommand(unitId, route);
    }

    public void ExecuteAttackCommand(ulong unitId, Vector2Int targetPosition)
    {
        _commandService.ExecuteAttackCommand(unitId, targetPosition);
    }

    public void ExecuteMoveThenAttackCommand(ulong unitId, List<Vector2Int> route, Vector2Int targetPosition)
    {
        _commandService.ExecuteMoveThenAttackCommand(unitId, route, targetPosition);
    }

    public void OnGameViewObjectSelected(IGameViewObject gameViewObject)
    {
        _worldToCellProvider.ToGrid(gameViewObject.transform.position, out var gridCoords);
        CellSelected?.Invoke(gridCoords);
        _gameModel.OnGameModelObjectSelected(gridCoords);
    }

    public void OnActionPerformed(IGameViewObject gameViewObject)
    {
        _worldToCellProvider.ToGrid(gameViewObject.transform.position, out var gridCoords);
        var cell = _gameModel.GetCell(gridCoords);
        if (cell?.Unit is UnitModel unit)
        {
            UnitStatsRequested?.Invoke(unit);
        }
    }

    private ActionPreview? DetermineActionPreview(Vector2Int cellCoords)
    {
        var active = _turnSystem != null && _turnSystem.ActiveObject != null ? _turnSystem.ActiveObject.Value : null;
        bool isMyTurn = _turnSystem != null && _turnSystem.IsMyTurn;
        if (active == null)
        {
            return new ActionPreview
            {
                MoveRoute = new List<Vector2Int>(),
                InaccessibleRoute = new List<Vector2Int>(),
                ReachableCells = ReachableCells,
                IsActionAvailable = false,
                IsMyTurn = isMyTurn,
                HoveredCell = cellCoords,
                Damage = null
            };
        }

        // Route to hovered
        var fullRoute = new List<Vector2Int>();
        if (active.Stats.CanFly)
            _movementSystem.GetRouteIgnoringObstacles(active.Position, cellCoords, out fullRoute);
        else
            _movementSystem.GetRoute(active.Position, cellCoords, out fullRoute);
        if (fullRoute.Count == 0)
        {
            return ActionPreview.notAvailable;
        }
        var moveRoute = _movementSystem.GetAccessibleRoutePoints(fullRoute, active.Stats.MoveSpeed) ?? new List<Vector2Int>();
        var inaccessRoute = fullRoute.Except(moveRoute).ToList();

        // Attack preview if enemy on cell
        DamageContext damage = null;
        bool canAttack = false;
        var cell = _gameModel.GetCell(cellCoords);
        if (cell != null && cell.Unit is ICombatObject targetObj && !ReferenceEquals(targetObj, active))
        {
            bool isEnemy = targetObj.IsBlueTeam != active.IsBlueTeam;
            if (isEnemy && _movementSystem.HasLineOfSight(active.Position, cellCoords))
            {
                if (active is IDamageSource source)
                {
                    if (targetObj is IDamagable targetDmg)
                    {
                        damage = source.SimulateSendDamage(new(targetDmg));
                    }
                    canAttack = damage != null;
                }
            }
        }

        return new ActionPreview
        {
            MoveRoute = moveRoute,
            InaccessibleRoute = inaccessRoute,
            ReachableCells = ReachableCells,
            IsActionAvailable = canAttack || (moveRoute != null && moveRoute.Count > 0),
            IsMyTurn = isMyTurn,
            HoveredCell = cellCoords,
            Damage = damage
        };
    }

    public IActionHandler ResolveAction(ICombatObject combatObject, Vector2Int hoveredCell, SpellData selectedSpell)
    {
        if (selectedSpell != null && combatObject is ISpellCaster caster)
            return _actionHandlerFactory.CreateSpellHandler(caster);

        var targetObj = _gameModel.GetCell(hoveredCell)?.Unit;
        if(targetObj is IDamagable enemy)
        {
            if(combatObject is IAttacker attacker && attacker.CanAttack)
                if(combatObject is IRangedAttacker rangedAttacker)
                {
                    if (_movementSystem.HasLineOfSight(combatObject.Position, hoveredCell))
                    {
                        _movementSystem.GetRouteIgnoringObstacles(combatObject.Position, hoveredCell, out var route);
                        var routeCost = _movementSystem.GetRouteCost(route);
                        if (rangedAttacker.AttackRange >= routeCost && routeCost > 1)
                        {
                            var rangedActionHandler = _actionHandlerFactory.CreateRangedAttackHandler(combatObject, enemy);
                            return rangedActionHandler;
                        }
                    }
                }
            else
            {
                _movementSystem.GetRoute(combatObject.Position, hoveredCell, out var moveRoute);
                
                return _actionHandlerFactory.CreateMoveThenAttackHandler(unit);
            }
        }

        if (unit is IMoveable)
            return _actionHandlerFactory.CreateMoveHandler(unit);

        return null;
    }


    public void Dispose()
    {
        // Отписка от событий модели
        _gameModel.UnitSpawned -= OnUnitSpawned;
        _gameModel.UnitDied -= OnUnitDied;
        _gameModel.UnitMovedByRoute -= OnUnitMovedByRoute;
        
        // Отписка от событий всех юнитов
        foreach (var kvp in _uvms)
        {
            if (kvp.Key is UnitModel unitModel)
            {
                unitModel.Attacked -= (context) => OnUnitAttacked(unitModel, context);
                unitModel.Hitted -= (context) => OnUnitHit(unitModel, context);
                unitModel.Died -= () => OnUnitDied(unitModel);
                unitModel.TurnStarted -= () => OnUnitTurnStarted(unitModel);
                unitModel.HealthChanged -= () => OnUnitHealthChanged(unitModel);
            }
        }
    }
}
public class GameViewModelDebugger : GameViewModel
{
    GameViewModelDebugger(GameModel model, MovementSystem movementSystem, CommandService commandService) : base(model, movementSystem,commandService) {
        Debug.Log("GameViewModel is ready");
    }
    protected override void OnUnitSpawned(UnitModelCreatedParams @params)
    {
        Debug.Log("GameViewModel OnUnitSpawned called " + @params.UnitModel.ToString());
        base.OnUnitSpawned(@params);
    }
}