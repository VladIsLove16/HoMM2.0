using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class UnitViewModel : IViewModel
{
    public UnitModel Model { get; }
    public Team Team => Model.Team.Value;
    public UnitType UnitType => Model.UnitType.Value;
    public IReadOnlyReactiveProperty<Team> TeamObservable => Model.Team;
    public IReadOnlyReactiveProperty<int> AmountObservable => Model.Amount;

    public IObservable<Unit> OnDeath => _onDeath;
    public IObservable<Unit> OnTurnStarted => _onTurnStarted;
    public IObservable<Unit> OnHealthChanged => _onHealthChanged;
    public IObservable<bool> OnTeamChanged => _onTeamChanged;
    public IObservable<Team> OnTeamChangedEnum => _onTeamChangedEnum;
    public IObservable<int> OnAmountChanged => _onAmountChanged;
    public IObservable<Vector2Int> OnPosChanged => _onPosChanged;
    public IObservable<List<Vector2Int>> OnMoveByRoute => _onMovedByRoute;

    public IObservable<Vector3> OnWorldPositionChanged => _onWorldPositionChanged;
    public IObservable<IReadOnlyList<Vector3>> OnMoveByWorldRoute => _onMovedByWorldRoute;
    public IObservable<Vector3> OnAttackedWorld => _onAttackedWorld;
    public IObservable<Vector3> OnHitWorld => _onHitWorld;

    public float HealthRatio => Model.ModifiedStats.MaxHealth > 0
        ? (float)Model.ModifiedStats.Health / Model.ModifiedStats.MaxHealth
        : 0f;

    public int Health => Model.ModifiedStats.Health;
    public int MaxHealth => Model.ModifiedStats.MaxHealth;
    public int Amount => Model.Amount.Value;

    private readonly IWorldToCellProvider _worldToCellProvider;

    private readonly Subject<Unit> _onDeath = new();
    private readonly Subject<Unit> _onTurnStarted = new();
    private readonly Subject<Unit> _onHealthChanged = new();
    private readonly Subject<bool> _onTeamChanged = new();
    private readonly Subject<Team> _onTeamChangedEnum = new();
    private readonly Subject<int> _onAmountChanged = new();
    private readonly Subject<Vector2Int> _onPosChanged = new();
    private readonly Subject<List<Vector2Int>> _onMovedByRoute = new();

    private readonly Subject<Vector3> _onWorldPositionChanged = new();
    private readonly Subject<IReadOnlyList<Vector3>> _onMovedByWorldRoute = new();
    private readonly Subject<Vector3> _onAttackedWorld = new();
    private readonly Subject<Vector3> _onHitWorld = new();

    public UnitViewModel(UnitModel model, IWorldToCellProvider worldToCellProvider)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _worldToCellProvider = worldToCellProvider ?? throw new ArgumentNullException(nameof(worldToCellProvider));

        SubscribeToModel();
    }

    private void SubscribeToModel()
    {
        Model.Attacked += ctx =>
        {
            EmitWorldPosition(ctx?.Target as IGridContent, _onAttackedWorld);
        };

        Model.Hitted += ctx =>
        {
            EmitWorldPosition(ctx?.Source as IGridContent, _onHitWorld);
        };

        Model.Died += () => _onDeath.OnNext(Unit.Default);
        Model.TurnStarted += () => _onTurnStarted.OnNext(Unit.Default);
        Model.HealthChanged += () => _onHealthChanged.OnNext(Unit.Default);

        Model.Amount.Subscribe(value => _onAmountChanged.OnNext(value));
        Model.Team.Subscribe(team =>
        {
            _onTeamChanged.OnNext(team == Team.Blue);
            _onTeamChangedEnum.OnNext(team);
        });

        Model.Position.Subscribe(position =>
        {
            _onPosChanged.OnNext(position);
            _onWorldPositionChanged.OnNext(ToWorldPosition(position));
        });

        Model.MovedByRoute += route =>
        {
            if (route == null)
            {
                return;
            }

            _onMovedByRoute.OnNext(route);
            var worldRoute = route.Select(ToWorldPosition).ToList();
            _onMovedByWorldRoute.OnNext(worldRoute);
        };
    }

    private Vector3 ToWorldPosition(Vector2Int coords)
    {
        return _worldToCellProvider?.ToWorld(coords.x, coords.y) ?? new Vector3(coords.x, 0f, coords.y);
    }

    private void EmitWorldPosition(IGridContent content, IObserver<Vector3> observer)
    {
        if (content == null || observer == null)
        {
            return;
        }

        observer.OnNext(ToWorldPosition(content.Position));
    }
}
