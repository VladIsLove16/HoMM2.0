using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public sealed class UnitPortraitViewModel : IDisposable
{
    private readonly ICombatObject _combatUnit;
    private readonly IGridViewModel _gridViewModel;
    private readonly GameViewModel _gameViewModel;
    private readonly UnitModel _unitModel;
    private readonly CompositeDisposable _disposables = new();
    private readonly ReactiveProperty<int> _turnOrder;
    private readonly ReactiveProperty<Team> _team;
    private readonly ReactiveProperty<Vector2Int> _cellPosition;
    private readonly Subject<UnitPortraitDamageEvent> _damageStream = new();
    [Inject] private ITurnQueue _turnQueue;
    public UnitPortraitViewModel(
        ICombatObject combatUnit,
        Sprite icon,
        int turnOrder,
        IGridViewModel gridViewModel,
        GameViewModel gameViewModel)
    {
        _combatUnit = combatUnit ?? throw new ArgumentNullException(nameof(combatUnit));
        _gridViewModel = gridViewModel ?? throw new ArgumentNullException(nameof(gridViewModel));
        _gameViewModel = gameViewModel ?? throw new ArgumentNullException(nameof(gameViewModel));

        Icon = icon;
        _turnOrder = new ReactiveProperty<int>(turnOrder);
        _team = new ReactiveProperty<Team>(combatUnit.Team);
        _unitModel = combatUnit as UnitModel;
        _cellPosition = new ReactiveProperty<Vector2Int>(combatUnit.Position);

        if (_unitModel != null)
        {
            _unitModel.Team.Subscribe(value => _team.Value = value).AddTo(_disposables);
            _unitModel.Position.Subscribe(pos => _cellPosition.Value = pos).AddTo(_disposables);
            _unitModel.Hitted += OnUnitHitted;
            _unitModel.Died += OnUnitDied;
        }

        _disposables.Add(_turnOrder);
        _disposables.Add(_team);
        _disposables.Add(_cellPosition);
    }

    public Sprite Icon { get; }
    public Team Team => _team.Value;
    public int TurnOrder => _turnOrder.Value;
    public ICombatObject CombatObject => _combatUnit;

    public IReadOnlyReactiveProperty<int> TurnOrderObservable => _turnOrder;
    public IReadOnlyReactiveProperty<Team> TeamObservable => _team;
    public IReadOnlyReactiveProperty<Vector2Int> CellPositionObservable => _cellPosition;
    public IObservable<UnitPortraitDamageEvent> DamageTaken => _damageStream;

    public event Action<UnitPortraitViewModel> UnitRemoved;

    public void UpdateTurnOrder(int turnOrder) => _turnOrder.Value = turnOrder;

    public void RequestFocus()
    {
        _gridViewModel?.HandleCellHovered((Vector2Int?)_cellPosition.Value);
    }

    public void ReleaseFocus()
    {
        _gridViewModel?.HandleCellHovered((Vector2Int?)null);
    }

    public void SelectUnit()
    {
        if (_gameViewModel == null)
            return;

        var cell = _cellPosition.Value;
        var coords = new KeyValuePair<Vector2Int, Vector2Int>(cell, cell);
        _gameViewModel.HandleCellSelected(coords);
    }

    private void OnUnitHitted(DamageContext ctx)
    {
        if (ctx == null)
        {
            return;
        }

        _damageStream.OnNext(new UnitPortraitDamageEvent(ctx.DamageAmount, ctx.DieAmount));
    }

    private void OnUnitDied()
    {
        ReleaseFocus();
        UnitRemoved?.Invoke(this);
        Dispose();
    }

    public void Dispose()
    {
        if (_unitModel != null)
        {
            _unitModel.Hitted -= OnUnitHitted;
            _unitModel.Died -= OnUnitDied;
        }

        _disposables.Dispose();
        _damageStream.OnCompleted();
        ReleaseFocus();
    }
}
