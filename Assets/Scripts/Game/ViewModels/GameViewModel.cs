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
    private ActionResolver _actionResolver;
    [Inject] private TurnSystem _turnSystem;
    [Inject] private UnitViewModelFactory _unitViewModelsFactory;

    public event Action<int, int> GridInitialized;
    public event Action<IViewModel> UnitSpawned;                        // глобальные широковещательные события
    public event Action<IViewModel, List<Vector2Int>> UnitMovedByRoute;    // глобальные широковещательные события
    public event Action<IViewModel, DamageContext> UnitAttacked;        // глобальные широковещательные события
    public event Action<IViewModel, DamageContext> UnitHit;             // глобальные широковещательные события
    public event Action<IViewModel> UnitDied;                           // глобальные широковещательные события
    public event Action<IViewModel> UnitTurnStarted;                    // глобальные широковещательные события
    [Obsolete("Для UI конкретного юнита используйте UnitViewModel.OnHealthChanged. Это событие предназначено для глобальных слушателей.")]
    public event Action<IViewModel> UnitHealthChanged;                  // глобальные; не использовать в Unit View/UI
    
    // Presentation layer events
    public event Action<UnitModel> UnitStatsRequested;                 // For UnitStatsPanel
    private Dictionary<IGridContent, IViewModel> _uvms = new Dictionary<IGridContent, IViewModel>();
    private List<Vector2Int> ReachableCells = new();

    public int ActiveUnitChanged { get; internal set; }

    public GameViewModel(GameModel model, MovementSystem movementSystem, CommandService commandService)
    {
        _gameModel = model;
        _movementSystem = movementSystem;
        _commandService = commandService;
        _actionResolver = new(model,movementSystem);

        _gameModel.UnitSpawned += OnUnitSpawned;
        _gameModel.UnitDied += OnUnitDied;
        _gameModel.UnitMovedByRoute += OnUnitMovedByRoute; 
        _gameModel.GridInitialized += g => GridInitialized?.Invoke(g.GetWidth(), g.GetHeight());
        if (_turnSystem != null && _turnSystem.ActiveObject != null)
        {
            _turnSystem.ActiveObject.Subscribe(_ => OnActiveUnitChanged());
        }
    }
    private object OnActiveUnitChanged()
    {
        if (_turnSystem.IsMyTurn)
        {

        }
    }
    public void HandleCellHovered(Vector2Int coords)
    {
        
    }

    public void HandleCellSelected(Vector2Int coords)
    {
        if (!_turnSystem.IsMyTurn)
            return;
        var fromCell = _turnSystem.ActiveObject.Value.Position;
        ActionContext actionContext = new(fromCell, coords, null);
        _actionResolver.Resolve(actionContext, out var actionHandler);
        actionHandler.Execute(actionContext);
    }

    public void HandleCellActionPerformed(Vector2Int coords)
    {
        var cell = _gameModel.GetCell(coords);
        if (cell?.Unit is UnitModel unit)
        {
            UnitStatsRequested?.Invoke(unit);
        }
    }

    private void OnUnitMovedByRoute(IGridContent content, List<Vector2Int> list)
    {
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