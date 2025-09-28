using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class UnitViewModel : IViewModel
{
    public UnitModel Model { get; }
    public IObservable<Unit> OnAttacked => _onAttacked;
    public IObservable<Unit> OnHit => _onHit;
    public IObservable<Unit> OnDeath => _onDeath;
    public IObservable<Unit> OnTurnStarted => _onTurnStarted;
    public IObservable<Unit> OnHealthChanged => _onHealthChanged;
    public IObservable<bool> OnTeamChanged => _onTeamChanged;
    public IObservable<Team> OnTeamChangedEnum => _onTeamChangedEnum;
    public IObservable<int> OnAmountChanged => _onAmountChanged;
    public IObservable<Vector2Int> OnPosChanged => _onPosChanged;
    public IObservable<List<Vector2Int>> OnMoveByRoute => _onMovedByRoute;

    public float HealthRatio => Model.ModifiedStats.MaxHealth > 0 ? (float)Model.ModifiedStats.Health / Model.ModifiedStats.MaxHealth : 0f;

    public int Health => Model.ModifiedStats.Health;

    public int MaxHealth => Model.ModifiedStats.MaxHealth;

    public int Amount => Model.Amount.Value;

    private Subject<List<Vector2Int>> _onMovedByRoute = new();
    private Subject<Unit> _onAttacked = new();
    private Subject<Unit> _onHit = new();
    private Subject<Unit> _onDeath = new();
    private Subject<Unit> _onTurnStarted = new();
    private Subject<Unit> _onHealthChanged = new();
    private Subject<bool> _onTeamChanged = new();
    private Subject<Team> _onTeamChangedEnum = new();
    private Subject<int> _onAmountChanged = new();
    private Subject<Vector2Int> _onPosChanged = new();
    public UnitViewModel(UnitModel model)
    {
        Model = model;

        SubscribeToModel();
    }

    private void SubscribeToModel()
    {
        // Пробрасываем события модели во ViewModel (MVVM)
        if (Model == null) return;

        Model.Attacked += _ => _onAttacked.OnNext(Unit.Default);
        Model.Hitted += _ => _onHit.OnNext(Unit.Default);
        Model.Died += () => _onDeath.OnNext(Unit.Default);
        Model.TurnStarted += () => _onTurnStarted.OnNext(Unit.Default);
        Model.HealthChanged += () => _onHealthChanged.OnNext(Unit.Default);
        Model.Amount.Subscribe(v => _onAmountChanged.OnNext(v));
        Model.Team.Subscribe(team =>
        {
            _onTeamChanged.OnNext(team == Team.Blue);
            _onTeamChangedEnum.OnNext(team);
        });
        Model.MovedByRoute+= (route) => _onMovedByRoute.OnNext(route); ;
    }
}
